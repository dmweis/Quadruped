namespace Quadruped.Driver
{
   class DynamixelErrorHelper
   {
      // Communication Result
      private const int COMM_SUCCESS = 0;       // tx or rx packet communication success
      private const int COMM_PORT_BUSY = -1000;   // Port is busy (in use)
      private const int COMM_TX_FAIL = -1001;   // Failed transmit instruction packet
      private const int COMM_RX_FAIL = -1002;   // Failed get status packet
      private const int COMM_TX_ERROR = -2000;   // Incorrect instruction packet
      private const int COMM_RX_WAITING = -3000;   // Now recieving status packet
      private const int COMM_RX_TIMEOUT = -3001;   // There is no status packet
      private const int COMM_RX_CORRUPT = -3002;   // Incorrect status packet
      private const int COMM_NOT_AVAILABLE = -9000;

      // Protocol 1.0 Error bit
      private const byte ERRBIT_VOLTAGE = 1;      // Supplied voltage is out of the range (operating volatage set in the control table)
      private const byte ERRBIT_ANGLE = 2;      // Goal position is written out of the range (from CW angle limit to CCW angle limit)
      private const byte ERRBIT_OVERHEAT = 4;      // Temperature is out of the range (operating temperature set in the control table)
      private const byte ERRBIT_RANGE = 8;      // Command(setting value) is out of the range for use.
      private const byte ERRBIT_CHECKSUM = 16;      // Instruction packet checksum is incorrect.
      private const byte ERRBIT_OVERLOAD = 32;      // The current load cannot be controlled by the set torque.
      private const byte ERRBIT_INSTRUCTION = 64;      // Undefined instruction or delivering the action command without the reg_write command.

      public static string GetTxRxResultDescription(int result)
      {
         switch (result)
         {
            case COMM_SUCCESS:
               return "[TxRxResult] Communication success.\n";
            case COMM_PORT_BUSY:
               return "[TxRxResult] Port is in use!\n";
            case COMM_TX_FAIL:
               return "[TxRxResult] Failed transmit instruction packet!\n";
            case COMM_RX_FAIL:
               return "[TxRxResult] Failed get status packet from device!\n";
            case COMM_TX_ERROR:
               return "[TxRxResult] Incorrect instruction packet!\n";
            case COMM_RX_WAITING:
               return "[TxRxResult] Now recieving status packet!\n";
            case COMM_RX_TIMEOUT:
               return "[TxRxResult] There is no status packet!\n";
            case COMM_RX_CORRUPT:
               return "[TxRxResult] Incorrect status packet!\n";
            case COMM_NOT_AVAILABLE:
               return "[TxRxResult] Protocol does not support This function!\n";
            default:
               return string.Empty;
         }
      }

      public static string GetRxPackErrorDescription(byte error)
      {
         if ((error & ERRBIT_VOLTAGE) != 0)
            return "[RxPacketError] Input voltage error!\n";
         if ((error & ERRBIT_ANGLE) != 0)
            return "[RxPacketError] Angle limit error!\n";
         if ((error & ERRBIT_OVERHEAT) != 0)
            return "[RxPacketError] Overheat error!\n";
         if ((error & ERRBIT_RANGE) != 0)
            return "[RxPacketError] Out of range error!\n";
         if ((error & ERRBIT_CHECKSUM) != 0)
            return "[RxPacketError] Checksum error!\n";
         if ((error & ERRBIT_OVERLOAD) != 0)
            return "[RxPacketError] Overload error!\n";
         if ((error & ERRBIT_INSTRUCTION) != 0)
            return "[RxPacketError] Instruction code error!\n";
         return string.Empty;
      }
   }
}
