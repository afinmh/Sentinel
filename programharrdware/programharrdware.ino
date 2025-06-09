#include <Wire.h>
#include <math.h>

#define MPU6050_I2C_ADDRESS 0x68
#define FREQ 30.0 // sample frequency in Hz
double gSensitivity = 65.5; // for 500 deg/s

double gx = 0, gy = 0, gz = 0;  // roll, pitch, yaw
double gyrX = 0, gyrY = 0, gyrZ = 0;
int16_t accX = 0, accY = 0, accZ = 0;

double gyrXoffs = -281.00, gyrYoffs = 18.00, gyrZoffs = -83.00;

// Button pins
const int btnPin1 = 2;  // push button 1 (momentary)
const int btnPin2 = 3;  // push button 2 (momentary)
const int togglePin = 4; // toggle button

// Variables for toggle button state
bool toggleState = false;
bool lastToggleReading = false;

void setup() {
  Serial.begin(38400);
  Wire.begin();

  pinMode(btnPin1, INPUT_PULLUP); // tombol aktif LOW jika ditekan
  pinMode(btnPin2, INPUT_PULLUP);
  pinMode(togglePin, INPUT_PULLUP);

  // Wake up MPU6050
  i2c_write_reg(MPU6050_I2C_ADDRESS, 0x6b, 0x00);
  i2c_write_reg(MPU6050_I2C_ADDRESS, 0x1a, 0x01); // low pass filter
  i2c_write_reg(MPU6050_I2C_ADDRESS, 0x1b, 0x08); // gyro range 500 deg/s

  uint8_t sample_div = 1000 / FREQ - 1;
  i2c_write_reg(MPU6050_I2C_ADDRESS, 0x19, sample_div);

  calibrate();
}

void loop() {
  unsigned long start_time = millis();

  read_sensor_data();

  // Hitung rotasi dari gyro (integrasi sudut per waktu)
  gx += gyrX / FREQ; // Roll
  gy += gyrY / FREQ; // Pitch
  gz += gyrZ / FREQ; // Yaw

  // Komplementary filter dengan accelerometer
  double acc_pitch = atan2(accY, sqrt(pow(accX, 2) + pow(accZ, 2))) * 180 / M_PI;
  double acc_roll  = atan2(accX, sqrt(pow(accY, 2) + pow(accZ, 2))) * 180 / M_PI;

  gx = gx * 0.96 + acc_roll * 0.04;
  gy = gy * 0.96 + acc_pitch * 0.04;

  // Baca tombol push button (active LOW)
  int btn1State = digitalRead(btnPin1) == LOW ? 1 : 0;
  int btn2State = digitalRead(btnPin2) == LOW ? 1 : 0;

  // Baca dan update toggle button (active LOW)
  bool toggleReading = digitalRead(togglePin) == LOW ? true : false;
  if (toggleReading && !lastToggleReading) {
    // Tombol baru saja ditekan, toggle state
    toggleState = !toggleState;
  }
  lastToggleReading = toggleReading;

  int toggleOut = toggleState ? 1 : 0;

  // Output data roll, pitch, yaw + tombol
  Serial.print(gx, 2); Serial.print(",");
  Serial.print(gy, 2); Serial.print(",");
  Serial.print(gz, 2); Serial.print(",");
  Serial.print(btn1State); Serial.print(",");
  Serial.print(btn2State); Serial.print(",");
  Serial.println(toggleOut);

  delay(((1 / FREQ) * 1000) - (millis() - start_time));
}

void calibrate() {
  long xSum = 0, ySum = 0, zSum = 0;
  uint8_t i2cData[6];
  int num = 500;

  for (int x = 0; x < num; x++) {
    if (i2c_read(MPU6050_I2C_ADDRESS, 0x43, i2cData, 6) != 0) return;

    xSum += ((i2cData[0] << 8) | i2cData[1]);
    ySum += ((i2cData[2] << 8) | i2cData[3]);
    zSum += ((i2cData[4] << 8) | i2cData[5]);
  }

  gyrXoffs = xSum / num;
  gyrYoffs = ySum / num;
  gyrZoffs = zSum / num;
}

void read_sensor_data() {
  uint8_t i2cData[14];
  if (i2c_read(MPU6050_I2C_ADDRESS, 0x3b, i2cData, 14) != 0) return;

  accX = ((i2cData[0] << 8) | i2cData[1]);
  accY = ((i2cData[2] << 8) | i2cData[3]);
  accZ = ((i2cData[4] << 8) | i2cData[5]);

  gyrX = (((i2cData[8] << 8) | i2cData[9]) - gyrXoffs) / gSensitivity;
  gyrY = (((i2cData[10] << 8) | i2cData[11]) - gyrYoffs) / gSensitivity;
  gyrZ = (((i2cData[12] << 8) | i2cData[13]) - gyrZoffs) / gSensitivity;
}

// I2C Functions
int i2c_read(int addr, int start, uint8_t *buffer, int size) {
  Wire.beginTransmission(addr);
  Wire.write(start);
  if (Wire.endTransmission(false) != 0) return -1;

  Wire.requestFrom(addr, size, true);
  for (int i = 0; i < size && Wire.available(); i++) {
    buffer[i] = Wire.read();
  }
  return 0;
}

int i2c_write(int addr, int start, const uint8_t *pData, int size) {
  Wire.beginTransmission(addr);
  Wire.write(start);
  Wire.write(pData, size);
  return Wire.endTransmission(true);
}

int i2c_write_reg(int addr, int reg, uint8_t data) {
  return i2c_write(addr, reg, &data, 1);
}
