using dCom.Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace dCom.Modbus.ModbusFunctions
{
	public class ReadCoilsFunction : ModbusFunction
	{
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
		{
			CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
		}

		/// <inheritdoc/>
		public override byte[] PackRequest()
		{
            byte[] request = new byte[12];
            byte[] temp = new byte[2];

            temp = BitConverter.GetBytes(CommandParameters.TransactionId);
            request[0] = temp[1];
            request[1] = temp[0];

            temp = BitConverter.GetBytes(CommandParameters.ProtocolId);
            request[2] = temp[1];
            request[3] = temp[0];

            temp = BitConverter.GetBytes(CommandParameters.Length);
            request[4] = temp[1];
            request[5] = temp[0];

            request[6] = CommandParameters.UnitId;
            request[7] = CommandParameters.FunctionCode;

            temp = BitConverter.GetBytes(((ModbusReadCommandParameters)CommandParameters).StartAddress);
            request[8] = temp[1];
            request[9] = temp[0];

            temp = BitConverter.GetBytes(((ModbusReadCommandParameters)CommandParameters).Quantity);
            request[10] = temp[1];
            request[11] = temp[0];

            return request;
		}

		/// <inheritdoc />
		public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
		{
            var retval = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters mbParams = (ModbusReadCommandParameters)CommandParameters;
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

                    retval.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, (ushort)(address + j)), value);
                }
            }

            return retval;
		}
	}
}