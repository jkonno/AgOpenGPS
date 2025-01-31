using Kvaser.CanLib;
using System;
using AgLibrary.Logging;
using System.Diagnostics;

public class CANHandler
{
    private readonly AgIO.FormLoop mf;
    private int handle;
    
    // CAN message IDs (ISOBUS)
    private const int PGN_CURVATURE_SPEED = 0xAD00;  // 44288 for guidance
    
    public CANHandler(AgIO.FormLoop _mf)
    {
        mf = _mf;
    }

    public bool InitCAN()
    {
        try
        {
            handle = Canlib.canOpenChannel(0, Canlib.canOPEN_ACCEPT_VIRTUAL);
            Canlib.canSetBusParams(handle, Canlib.canBITRATE_250K, 0, 0, 0, 0);
            Canlib.canBusOn(handle);
            return true;
        }
        catch (Exception ex)
        {
            Log.EventWriter("CAN Init: " + ex.Message);
            return false;
        }
    }

    public void SendPgnToCANBus(byte[] data)
    {
        try
        {
            // Check header
            if (data[0] != 0x80 || data[1] != 0x81) return;

            byte[] msg = new byte[8];
            
            switch (data[3])  // PGN ID
            {
                case 0xE6:  // 230 Curvature/Speed
                    // Copy data directly from package
                    msg[0] = data[8];  // Curvature low byte
                    msg[1] = data[9];  // Curvature high byte
                    msg[2] = data[5];  // Speed low byte
                    msg[3] = data[6];  // Speed high byte
                    
                    Canlib.canWrite(handle, PGN_CURVATURE_SPEED, msg, 8, Canlib.canMSG_STD);
                    break;

                // Add other PGN cases as needed
                //case 0xFE:  // 254 AutoSteer
                //    break;
            }
        }
        catch (Exception ex)
        {
            Log.EventWriter("CAN Send: " + ex.Message);
        }
    }

    public void Close()
    {
        try
        {
            Canlib.canBusOff(handle);
            Canlib.canClose(handle);
        }
        catch { }
    }
}