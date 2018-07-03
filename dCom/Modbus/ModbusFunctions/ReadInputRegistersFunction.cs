using dCom.Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace dCom.Modbus.ModbusFunctions
{
	public class ReadInputRegistersFunction : ModbusFunction
	{
		public ReadInputRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
		{
			CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
		}

        
		/// <inheritdoc />
		public override byte[] PackRequest()
		{/*
            ModbusReadCommandParameters para = this.CommandParameters as ModbusReadCommandParameters;
            byte[] ret = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)para.TransactionId)), 0, ret, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)para.ProtocolId)), 0, ret, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)para.Length)), 0, ret, 4, 2);

            ret[6] = para.UnitId;
            ret[7] = para.FunctionCode;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)para.StartAddress)), 0, ret, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)para.Quantity)), 0, ret, 10, 2);

            return ret;
            */
            ModbusReadCommandParameters mdbParams = (ModbusReadCommandParameters)CommandParameters;
            byte[] req = new byte[12];
            
            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)CommandParameters.ProtocolId)), 0, req, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)CommandParameters.Length)), 0, req, 4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)CommandParameters.TransactionId)), 0, req, 0, 2);
            req[6] = mdbParams.UnitId;
            req[7] = mdbParams.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)mdbParams.StartAddress)), 0, req, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)mdbParams.Quantity)), 0, req, 10, 2);
            
            return req;        
		}

		/// <inheritdoc />
		public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
		{
            ModbusReadCommandParameters mdbParams = (ModbusReadCommandParameters)CommandParameters;
            var retval = new Dictionary<Tuple<PointType, ushort>, ushort>();

            ushort address = mdbParams.StartAddress;
            int quantity = response[8];

            for(int i = 0, j = 0; i < quantity; i += 2, ++j)
            {
                ushort byte1, byte2, value;
                byte1 = response[9 + i];
                byte2 = response[10 + i];

                value = (ushort)(byte2 + (byte1 << 8));
                retval.Add(new Tuple<PointType, ushort>(PointType.ANALOG_INPUT, (ushort)(address + j)), value);
            }

            return retval;
		}
	}
}