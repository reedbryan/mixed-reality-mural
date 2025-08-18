#include <WiFi.h>
#include <WiFiUdp.h>
#include <OSCMessage.h>

const char* ssid     = "Signals-Guest";
const char* password = "DigiBC2024";

IPAddress outIp; // <-- will be set dynamically
const unsigned int outPort = 9000;

WiFiUDP Udp;

const int touchPins[8] = {4, 5, 6, 7, 8, 9, 10, 11};
const int ledPins[8]   = {36, 37, 38, 39, 40, 41, 48, 47};
const int touchThreshold = 70000;

IPAddress getBroadcastAddress(IPAddress ip, IPAddress subnet) {
  IPAddress broadcast;
  for (int i = 0; i < 4; i++) {
    broadcast[i] = ip[i] | ~subnet[i];
  }
  return broadcast;
}

void setup() {
  Serial.begin(115200);
  delay(1000);

  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\nWiFi connected");

  // Dynamically determine broadcast address
  IPAddress localIP = WiFi.localIP();
  IPAddress subnetMask = WiFi.subnetMask();
  outIp = getBroadcastAddress(localIP, subnetMask);

  Serial.print("Local IP: "); Serial.println(localIP);
  Serial.print("Subnet: "); Serial.println(subnetMask);
  Serial.print("Broadcast IP: "); Serial.println(outIp);

  for (int i = 0; i < 8; i++) {
    pinMode(ledPins[i], OUTPUT);
    digitalWrite(ledPins[i], LOW);
  }

  Udp.begin(WiFi.localIP(), outPort); // bind UDP port if needed
}


void loop() {
  int activeIndex = -1;

  for (int i = 0; i < 8; i++) {
    int touchValue = touchRead(touchPins[i]);
    Serial.print("T");
    Serial.print(i);
    Serial.print(" (GPIO ");
    Serial.print(touchPins[i]);
    Serial.print("): ");
    Serial.print(touchValue);
    Serial.print("\t");

    if (touchValue > touchThreshold) {
      digitalWrite(ledPins[i], HIGH);
      if (activeIndex == -1) activeIndex = i;
    } else {
      digitalWrite(ledPins[i], LOW);
    }
  }

  Serial.println();

  OSCMessage msg("/camera");
  msg.add(activeIndex); // -1 = default camera

  Udp.beginPacket(outIp, outPort);
  msg.send(Udp);
  Udp.endPacket();
  msg.empty();

  delay(50);
}
