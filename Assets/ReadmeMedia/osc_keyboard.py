#!/usr/bin/env python3
import os
import sys
import time
import socket
import struct
import argparse

PORT_DEFAULT = 9000
TICK_MS_DEFAULT = 50

# ------------- OSC packing (no external deps) -------------
def _pad4(b: bytes) -> bytes:
    return b + (b"\x00" * ((4 - (len(b) % 4)) % 4))

def osc_pack(address: str, *args) -> bytes:
    """
    Minimal OSC 1.0 message packer for our use:
    - address: string like "/camera"
    - args: only supports integers here
    """
    if not address.startswith("/"):
        raise ValueError("OSC address must start with '/'")
    addr_bin = _pad4(address.encode("utf-8") + b"\x00")

    # Build typetags (only ints in this script)
    typetags = "," + "".join("i" for _ in args)
    tags_bin = _pad4(typetags.encode("utf-8") + b"\x00")

    data = b""
    for a in args:
        if not isinstance(a, int):
            raise TypeError("Only integer args are supported in this script.")
        data += struct.pack(">i", a)  # big-endian 32-bit int
    return addr_bin + tags_bin + data

# ------------- Target discovery -------------
def discover_broadcast_targets():
    """
    Try to discover per-interface broadcast addresses using 'netifaces' if available.
    Fallback to universal broadcast 255.255.255.255.
    """
    targets = set()
    try:
        import netifaces  # optional
        for iface in netifaces.interfaces():
            addrs = netifaces.ifaddresses(iface).get(netifaces.AF_INET, [])
            for a in addrs:
                bcast = a.get("broadcast")
                if bcast and bcast != "127.255.255.255":
                    targets.add(bcast)
    except ImportError:
        # No netifaces? Use universal broadcast and hope the OS/router allows it.
        pass

    if not targets:
        targets.add("255.255.255.255")
    return sorted(targets)

# ------------- Keyboard (cross-platform) -------------
class KeyReader:
    def __init__(self):
        self.win = (os.name == "nt")
        if self.win:
            import msvcrt  # noqa: F401
            self.kbhit = msvcrt.kbhit
            self.getch = msvcrt.getch
        else:
            import tty, termios, select
            self._tty = tty
            self._termios = termios
            self._select = select
            self.fd = sys.stdin.fileno()
            self.old_settings = termios.tcgetattr(self.fd)
            tty.setcbreak(self.fd)

    def read_key(self):
        """
        Non-blocking: returns a single decoded char if available, else None.
        """
        if self.win:
            import msvcrt
            if msvcrt.kbhit():
                ch = msvcrt.getch()
                try:
                    return ch.decode("utf-8", errors="ignore")
                except Exception:
                    return None
            return None
        else:
            dr, _, _ = self._select.select([sys.stdin], [], [], 0)
            if dr:
                ch = os.read(self.fd, 1)
                try:
                    return ch.decode("utf-8", errors="ignore")
                except Exception:
                    return None
            return None

    def close(self):
        if not self.win:
            self._termios.tcsetattr(self.fd, self._termios.TCSADRAIN, self.old_settings)

# ------------- Sender -------------
class OSCSender:
    def __init__(self, targets, port):
        self.targets = targets
        self.port = port
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        # Allow broadcast
        self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
        # Optional: allow reuse
        try:
            self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        except Exception:
            pass

    def send(self, data: bytes):
        for t in self.targets:
            try:
                self.sock.sendto(data, (t, self.port))
            except OSError as e:
                # Don't crash if one target fails; continue with others
                print(f"[warn] sendto {t}:{self.port} failed: {e}", file=sys.stderr)

# ------------- Main -------------
def main():
    ap = argparse.ArgumentParser(description="Keyboard->OSC sender (broadcast-aware)")
    ap.add_argument("--port", type=int, default=PORT_DEFAULT, help=f"OSC UDP port (default {PORT_DEFAULT})")
    ap.add_argument("--tick-ms", type=int, default=TICK_MS_DEFAULT, help=f"Loop delay in ms (default {TICK_MS_DEFAULT})")
    ap.add_argument("--target", type=str, default=os.environ.get("OSC_TARGET", "").strip(),
                    help="Unicast target IP or hostname (overrides broadcast). You can also set OSC_TARGET env var.")
    args = ap.parse_args()

    if args.target:
        targets = [args.target]
        mode = "unicast"
    else:
        targets = discover_broadcast_targets()
        mode = "broadcast"

    print(f"Mode: {mode}")
    print(f"Targets: {', '.join(targets)}")
    print(f"Port: {args.port}")
    print("Controls: press 1–8 to send that index (0–7). Press 'q' to quit.")
    print("Note: When idle, the script sends -1.")

    sender = OSCSender(targets, args.port)
    kb = KeyReader()

    try:
        while True:
            idx = -1  # default camera when idle
            key = kb.read_key()
            if key:
                if key.lower() == "q":
                    break
                if key.isdigit():
                    n = int(key)
                    if 1 <= n <= 8:
                        idx = n - 1
                        print(f"/camera {idx}")
                    else:
                        # ignore other digits
                        pass

            payload = osc_pack("/camera", idx)
            sender.send(payload)
            time.sleep(max(0.0, args.tick_ms / 1000.0))
    finally:
        kb.close()
        print("Stopped.")

if __name__ == "__main__":
    main()
