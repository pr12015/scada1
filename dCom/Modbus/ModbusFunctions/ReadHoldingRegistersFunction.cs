using dCom.Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace dCom.Modbus.ModbusFunctions
{
	public class ReadHoldingRegistersFunction : ModbusFunction
	{
		public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
		{
			CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
		}

		/// <inheritdoc />
		public override byte[] PackRequest()
		{
            byte[] req = new byte[12];
            ModbusReadCommandParameters mbParams = (ModbusReadCommandParameters)CommandParameters;

            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)mbParams.TransactionId)), 0, req, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)mbParams.ProtocolId)), 0, req, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)mbParams.Length)), 0, req, 4, 2);
            req[6] = mbParams.UnitId;
            req[7] = mbParams.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)mbParams.StartAddress)), 0, req, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)mbParams.Quantity)), 0, req, 10, 2);

            return req;
		}

		/// <inheritdoc />
		public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
		{
            ModbusReadCommandParameters mbParams = (ModbusReadCommandParameters)CommandParameters;
            var retval = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ushort address = mbParams.StartAddress;

            int quantity = response[8];
            ushort byte1, byte2, value;
            for (int i = 0, j = 0; i < quantity; i += 2, ++j)
            {
                byte1 = response[9 + i];
                byte2 = response[10 + i];
                value = (ushort)(byte2 + (byte1 << 8));

                retval.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, (ushort)(address + j)), value);
            }

            return retval;
		}
	}
}