#include <TimerOne.h>

// Clears a bit
#ifndef cbi
  #define cbi(sfr, bit) (_SFR_BYTE(sfr)&=~_BV(bit))
#endif

// Sets a bit
#ifndef sbi
  #define sbi(sfr, bit) (_SFR_BYTE(sfr)|=_BV(bit))
#endif

// Examines a bit
#ifndef ebi
  #define ebi(var, bit) (var&(1<<bit))
#endif

#define MINIMUM_POSITION 0
// Maximum number of steps for one direction
#define MAXIMUM_POSITION 158

// Timer resolution
#define RESOLUTION 40

// floppy head positions
byte currentPosition[]={0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};

unsigned int currentPeriod[]={0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
unsigned int currentTick[]={0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
 
 // For Arduino Mega only:
 //
 // PORTA = Pins: 22, 23, 24, 25, 26, 27, 28, 29
 // PORTB = Pins: 10, 11, 12, 13, 50, 51, 52, 53
 // PORTC = Pins: 30, 31, 32, 33, 34, 35, 36, 37
 // PORTD = Pins: 18, 19, 20, 21, 38
 // PORTE = Pins: 0, 1, 2, 3, 5
 // PORTF = Pins: 54, 55, 56, 57, 58, 59, 60, 61
 // PORTG = Pins: 4, 39, 40, 41
 // PORTH = Pins: 16, 17, 6, 7, 8, 9
 // PORTJ = Pins: 14, 15
 // PORTK = Pins: 62, 63, 64, 65, 66, 67, 68, 69
 // PORTL = Pins: 42, 43, 44, 45, 46, 47, 48, 49
 
 byte porta = 0;
 byte portadirection = 0;
 byte portc = 0;
 byte portcdirection = 0;
 byte portf = 0;
 byte portfdirection = 0;
 byte portl = 0;
 byte portldirection = 0;

void setup(){
  
  //Initialize all pins as outputs
  DDRA = DDRA|B11111111;
  DDRC = DDRC|B11111111;
  DDRL = DDRC|B11111111;
  DDRF = DDRF|B11111111;
  
  //Initialize all the output registers
  PORTA = porta;
  PORTC = portc;
  PORTL = portl;
  PORTF = portf;
  
  Timer1.initialize(RESOLUTION); 
  Timer1.attachInterrupt(tick);

  Serial.begin(115200);
}

void loop()
{
  if(Serial.available() > 2)
  {
    if(Serial.peek() == 100)
    {
      resetAll();
      while(Serial.available() > 0)
      {
        Serial.read();
      }
    }
    else
    {
      currentPeriod[Serial.read()] = (Serial.read() << 8) | Serial.read();
    }
  }
}

void tick()
{
  // Drive 01
  if(currentPeriod[1] > 0)
  {
    currentTick[1]++;
    if(currentTick[1] >= currentPeriod[1])
    {
      if(currentPosition[1] >= MAXIMUM_POSITION)
      {
        sbi(porta, 1);
        sbi(portadirection, 1);
      }
      else if(currentPosition[1] <= MINIMUM_POSITION)
      {
        cbi(porta, 1);
        cbi(portadirection, 1);
      }

      //Update currentPosition
      if(ebi(porta, 1))
        currentPosition[1]--;
      else
        currentPosition[1]++;

      //Pulse the step pin
      if(ebi(porta, 0))
        cbi(porta, 0);
      else
        sbi(porta, 0);

      currentTick[1]=0;
    }
  }
    
  // Drive 02
  if(currentPeriod[2] > 0)
  {
    currentTick[2]++;
    if(currentTick[2] >= currentPeriod[2])
    {
      if(currentPosition[2] >= MAXIMUM_POSITION)
      {
        sbi(porta, 3);
        sbi(portadirection, 3);
      }
      else if(currentPosition[2] <= MINIMUM_POSITION)
      {
        cbi(porta, 3);
        cbi(portadirection, 3);
      }
      //Update currentPosition
      if(ebi(porta, 3))
        currentPosition[2]--;
      else
        currentPosition[2]++;

      //Pulse the step pin
      if(ebi(porta, 2))
        cbi(porta, 2);
      else
        sbi(porta, 2);

      currentTick[2] = 0;
    }
  }
    
  // Drive 03
  if(currentPeriod[3] > 0)
  {
    currentTick[3]++;
    if(currentTick[3] >= currentPeriod[3])
    {
      if(currentPosition[3] >= MAXIMUM_POSITION)
      {
        sbi(porta, 5);
        sbi(portadirection, 5);
      }
      else if(currentPosition[3] <= MINIMUM_POSITION)
      {
        cbi(porta, 5);
        cbi(portadirection, 5);
      }
      //Update currentPosition
      if(ebi(porta, 5))
        currentPosition[3]--;
      else
        currentPosition[3]++;

      //Pulse the step pin
      if(ebi(porta, 4))
        cbi(porta, 4);
      else
        sbi(porta, 4);

      currentTick[3] = 0;
    }
  }
    
  // Drive 04
  if(currentPeriod[4] > 0)
  {
    currentTick[4]++;
    if(currentTick[4] >= currentPeriod[4])
    {
      if(currentPosition[4] >= MAXIMUM_POSITION)
      {
        sbi(porta, 7);
        sbi(portadirection, 7);
      }
      else if(currentPosition[4] <= MINIMUM_POSITION)
      {
        cbi(porta, 7);
        cbi(portadirection, 7);
      }
      //Update currentPosition
      if(ebi(porta, 7))
        currentPosition[4]--;
      else
        currentPosition[4]++;

      //Pulse the step pin
      if(ebi(porta, 6))
        cbi(porta, 6);
      else
        sbi(porta, 6);

      currentTick[4] = 0;
    }
  }
  
  // Drive 05
  if(currentPeriod[5] > 0)
  {
    currentTick[5]++;
    if(currentTick[5] >= currentPeriod[5])
    {
      if(currentPosition[5] >= MAXIMUM_POSITION)
      {
        sbi(portc, 1);
        sbi(portcdirection, 1);
      }
      else if(currentPosition[5] <= MINIMUM_POSITION)
      {
        cbi(portc, 1);
        cbi(portcdirection, 1);
      }

      //Update currentPosition
      if(ebi(portc, 1))
        currentPosition[5]--;
      else
        currentPosition[5]++;

      //Pulse the step pin
      if(ebi(portc, 0))
        cbi(portc, 0);
      else
        sbi(portc, 0);

      currentTick[5] = 0;
    }
  }
    
  // Drive 06
  if(currentPeriod[6] > 0)
  {
    currentTick[6]++;
    if(currentTick[6] >= currentPeriod[6])
    {
      if(currentPosition[6] >= MAXIMUM_POSITION)
      {
        sbi(portc, 3);
        sbi(portcdirection, 3);
      }
      else if(currentPosition[6] <= MINIMUM_POSITION)
      {
        cbi(portc, 3);
        cbi(portcdirection, 3);
      }
      //Update currentPosition
      if(ebi(portc, 3))
        currentPosition[6]--;
      else
        currentPosition[6]++;

      //Pulse the step pin
      if(ebi(portc, 2))
        cbi(portc, 2);
      else
        sbi(portc, 2);

      currentTick[6] = 0;
    }
  }
    
  // Drive 07
  if(currentPeriod[7] > 0)
  {
    currentTick[7]++;
    if(currentTick[7] >= currentPeriod[7])
    {
      if(currentPosition[7] >= MAXIMUM_POSITION)
      {
        sbi(portc, 5);
        sbi(portcdirection, 5);
      }
      else if(currentPosition[7] <= MINIMUM_POSITION)
      {
        cbi(portc, 5);
        cbi(portcdirection, 5);
      }
      //Update currentPosition
      if(ebi(portc, 5))
        currentPosition[7]--;
      else
        currentPosition[7]++;

      //Pulse the step pin
      if(ebi(portc, 4))
        cbi(portc, 4);
      else
        sbi(portc, 4);

      currentTick[7] = 0;
    }
  }
    
  // Drive 08
  if(currentPeriod[8] > 0)
  {
    currentTick[8]++;
    if(currentTick[8] >= currentPeriod[8])
    {
      if(currentPosition[8] >= MAXIMUM_POSITION)
      {
        sbi(portc, 7);
        sbi(portcdirection, 7);
      }
      else if(currentPosition[8] <= MINIMUM_POSITION)
      {
        cbi(portc, 7);
        cbi(portcdirection, 7);
      }
      //Update currentPosition
      if(ebi(portc, 7))
        currentPosition[8]--;
      else
        currentPosition[8]++;

      //Pulse the step pin
      if(ebi(portc, 6))
        cbi(portc, 6);
      else
        sbi(portc, 6);

      currentTick[8] = 0;
    }
  }
    
  // Drive 09
  if(currentPeriod[9] > 0)
  {
    currentTick[9]++;
    if(currentTick[9] >= currentPeriod[9])
    {
      if(currentPosition[9] >= MAXIMUM_POSITION)
      {
        sbi(portl, 1);
        sbi(portldirection, 1);
      }
      else if(currentPosition[9] <= MINIMUM_POSITION)
      {
        cbi(portl, 1);
        cbi(portldirection, 1);
      }
      //Update currentPosition
      if(ebi(portl, 1))
        currentPosition[9]--;
      else
        currentPosition[9]++;

      //Pulse the step pin
      if(ebi(portl, 0))
        cbi(portl, 0);
      else
        sbi(portl, 0);

      currentTick[9] = 0;
    }
  } 
    
  // Drive 10
  if(currentPeriod[10] > 0)
  {
    currentTick[10]++;
    if(currentTick[10] >= currentPeriod[10])
    {
      if(currentPosition[10] >= MAXIMUM_POSITION)
      {
        sbi(portl, 3);
        sbi(portldirection, 3);
      }
      else if(currentPosition[10] <= MINIMUM_POSITION)
      {
        cbi(portl, 3);
        cbi(portldirection, 3);
      }
      //Update currentPosition
      if(ebi(portl, 3))
        currentPosition[10]--;
      else
        currentPosition[10]++;

      //Pulse the step pin
      if(ebi(portl, 2))
        cbi(portl, 2);
      else
        sbi(portl, 2);

      currentTick[10] = 0;
    }
  } 
    
  // Drive 11
  if(currentPeriod[11] > 0)
  {
    currentTick[11]++;
    if(currentTick[11] >= currentPeriod[11])
    {
      if(currentPosition[11] >= MAXIMUM_POSITION)
      {
        sbi(portl, 5);
        sbi(portldirection, 5);
      }
      else if(currentPosition[11] <= MINIMUM_POSITION)
      {
        cbi(portl, 5);
        cbi(portldirection, 5);
      }
      //Update currentPosition
      if(ebi(portl, 5))
        currentPosition[11]--;
      else
        currentPosition[11]++;

      //Pulse the step pin
      if(ebi(portl, 4))
        cbi(portl, 4);
      else
        sbi(portl, 4);

      currentTick[11] = 0;
    }
  } 
    
  // Drive 12
  if(currentPeriod[12] > 0)
  {
    currentTick[12]++;
    if(currentTick[12] >= currentPeriod[12])
    {
      if(currentPosition[12] >= MAXIMUM_POSITION)
      {
        sbi(portl, 7);
        sbi(portldirection, 7);
      }
      else if(currentPosition[12] <= MINIMUM_POSITION)
      {
        cbi(portl, 7);
        cbi(portldirection, 7);
      }
      //Update currentPosition
      if(ebi(portl, 7))
        currentPosition[12]--;
      else
        currentPosition[12]++;

      //Pulse the step pin
      if(ebi(portl, 6))
        cbi(portl, 6);
      else
        sbi(portl, 6);

      currentTick[12] = 0;
    }
  } 
    
  // Drive 13
  if(currentPeriod[13] > 0)
  {
    currentTick[13]++;
    if(currentTick[13] >= currentPeriod[13])
    {
      if(currentPosition[13] >= MAXIMUM_POSITION)
      {
        sbi(portf, 1);
        sbi(portfdirection, 1);
      }
      else if(currentPosition[13] <= MINIMUM_POSITION)
      {
        cbi(portf, 1);
        cbi(portfdirection, 1);
      }
      //Update currentPosition
      if(ebi(portf, 1))
        currentPosition[13]--;
      else
        currentPosition[13]++;

      //Pulse the step pin
      if(ebi(portf, 0))
        cbi(portf, 0);
      else
        sbi(portf, 0);

      currentTick[13] = 0;
    }
  } 
    
  // Drive 14
  if(currentPeriod[14] > 0)
  {
    currentTick[14]++;
    if(currentTick[14] >= currentPeriod[14])
    {
      if(currentPosition[14] >= MAXIMUM_POSITION)
      {
        sbi(portf, 3);
        sbi(portfdirection, 3);
      }
      else if(currentPosition[14] <= MINIMUM_POSITION)
      {
        cbi(portf, 3);
        cbi(portfdirection, 3);
      }
      //Update currentPosition
      if(ebi(portf, 3))
        currentPosition[14]--;
      else
        currentPosition[14]++;

      //Pulse the step pin
      if(ebi(portf, 2))
        cbi(portf, 2);
      else
        sbi(portf, 2);

      currentTick[14] = 0;
    }
  } 
    
  // Drive 15
  if(currentPeriod[15] > 0)
  {
    currentTick[15]++;
    if(currentTick[15] >= currentPeriod[15])
    {
      if(currentPosition[15] >= MAXIMUM_POSITION)
      {
        sbi(portf, 5);
        sbi(portfdirection, 5);
      }
      else if(currentPosition[15] <= MINIMUM_POSITION)
      {
        cbi(portf, 5);
        cbi(portfdirection, 5);
      }
      //Update currentPosition
      if(ebi(portf, 5))
        currentPosition[15]--;
      else
        currentPosition[15]++;

      //Pulse the step pin
      if(ebi(portf, 4))
        cbi(portf, 4);
      else
        sbi(portf, 4);

      currentTick[15] = 0;
    }
  } 
    
  // Drive 16
  if(currentPeriod[16] > 0)
  {
    currentTick[16]++;
    if(currentTick[16] >= currentPeriod[16])
    {
      if(currentPosition[16] >= MAXIMUM_POSITION)
      {
        sbi(portf, 7);
        sbi(portfdirection, 7);
      }
      else if(currentPosition[16] <= MINIMUM_POSITION)
      {
        cbi(portf, 7);
        cbi(portfdirection, 7);
      }
      //Update currentPosition
      if(ebi(portf, 7))
        currentPosition[16]--;
      else
        currentPosition[16]++;

      //Pulse the step pin
      if(ebi(portf, 6))
        cbi(portf, 6);
      else
        sbi(portf, 6);

      currentTick[16] = 0;
    }
  } 
    
  PORTA = portadirection;
  PORTC = portcdirection;
  PORTF = portfdirection;
  PORTL = portldirection;
  
  PORTA = porta;
  PORTC = portc;
  PORTF = portf;
  PORTL = portl;
  
  portadirection = porta;
  portcdirection = portc;
  portfdirection = portf;
  portldirection = portl;
}

void resetAll(){
  
  //Interrupts must be disabled while resetting
  cli();
  
  //Set all pins to reverse direction
  PORTA = B10101010;
  PORTC = B10101010;
  PORTF = B10101010;
  PORTL = B10101010;
  delay(5);
  
  // Set step pins
  for(byte s=0;s<80;s++){
    PORTA = B11111111;
    PORTC = B11111111;
    PORTF = B11111111;
    PORTL = B11111111;
    delay(100);

    PORTA = B10101010;
    PORTC = B10101010;
    PORTF = B10101010;
    PORTL = B10101010;
    delay(100);
  }
  
  // Reset status arrays
  for (byte p=0;p<=16;p+=1){
    currentPosition[p]=0;
    currentPeriod[p]=0;
    currentTick[p]=0;
  }
  
  // Reset outputs to forward
  PORTA = porta = portadirection = B00000000;
  PORTC = portc = portcdirection = B00000000;
  PORTF = portf = portfdirection = B00000000;
  PORTL = portl = portldirection = B00000000;
  
  // Re-enable interrupts
  sei();
}
