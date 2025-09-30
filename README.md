# Mixed Reality Mural Component

Mixed Reality Unity project uses extOSC and OpenXR to show/trigger/sync animated 3d assets and particle effects in a immersive mural instatallation

## Mural overview:
This mural experience is driven by touch copasitive pins on esp32s. Each esp32 has 4 of its touch pins connected to conductive paint, which when touched will activate the pin and send out a signed OSC signal to the network's broadcast IP.

The OSC signals send from the mircocontrollers drive different animation events throughout the installation, including those in this project (which drives the quest3 headsets). Users interacting with the mural with a headset on will see a center piece 3d model, with particle effects surronding. Each touch pin on the esp coorestponds to a different model with a different effect, so as different art pieces in the mural are touched, the mixed reality elements change. This effect is also synced through all headsets because the signal is being sent to the broadcast IP.

## Open Sound Control (OSC):
Open Sound Control (OSC) is a networking protocol designed for fast, flexible communication between computers, devices, and software. It was originally developed for musicians and performers to control sound synthesizers, but it has since become widely used in interactive and immersive installations.

### Process:
1. The ESP32 first connects to your Wi-Fi network using the SSID/password you provided.
2. Once connected, it obtains:
    a. Local IP (its own address on the network)
    b. Subnet Mask
3. From these, it calculates the broadcast IP → the address that sends packets to all devices on the local network (e.g. 192.168.1.255). 
'''c
IPAddress getBroadcastAddress(IPAddress ip, IPAddress subnet) {
  IPAddress broadcast;
  for (int i = 0; i < 4; i++) {
    broadcast[i] = ip[i] | ~subnet[i];
  }
  return broadcast;
}'''
That broadcast IP is stored in outIp and will be used as the target for OSC messages, allowing triggered effects to be synced between all headsets on the specified network.

### Builds
- See [ESP32 Code]("https://github.com/reedbryan/mixed-reality-mural\Assets\ReadmeMedia\board-with-broadcastIP.ino") (uploaded via arduino IDE)
- See [Python Code]("https://github.com/reedbryan/mixed-reality-mural\Assets\ReadmeMedia\osc_keyboard.py") (can be run from the terminal, for debugging purposes)

## Unity Side
The Unity project running inside the headsets (this repo) 

### Scene Specific Object

## Assets

### Totom Pole
[](https://github.com/reedbryan/mixed-reality-mural\Assets\ReadmeMedia\TotemSC.png)

### Pollinator
[](https://github.com/reedbryan/mixed-reality-mural\Assets\ReadmeMedia\BeeSC2.png)

### Cedar Tree
[](https://github.com/reedbryan/mixed-reality-mural\Assets\ReadmeMedia\TreeSC.png)