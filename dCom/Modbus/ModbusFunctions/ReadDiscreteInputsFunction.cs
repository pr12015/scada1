using dCom.Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace dCom.Modbus.ModbusFunctions
{
	public class ReadDiscreteInputsFunction : ModbusFunction
	{
		public ReadDiscreteInputsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
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
            ushort address = mbParams.StartAddress, value;

            int quantity = response[8];
            for(int i = 0; i < quantity; ++i)
            {
                for(int j = 0; j < 8; ++j)
                {
                    if ((j + i * 8) >= mbParams.Quantity)
                        break;

                    value = (ushort)(response[9 + i] & 0x1);
                    response[9 + i] /= 2; // >>> 1

                    retval.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_INPUT, (ushort)(address + j)), value);
                }
            }
            return retval;
		}
	}
}