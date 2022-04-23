void setup()
{
  ledcSetup(0, 12800, 10);
  ledcAttachPin(A12, 0)
}

int a = 0;

void loop()
{
    AnalogOutput( A12 , a );
}


void AnalogOutput(int pin, int power)
{
    power = power * 10.23;
    ledcWrite( pin, power );
}