using dCom.Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace dCom.Modbus.ModbusFunctions
{
	public class WriteSingleCoilFunction : ModbusFunction
	{
		public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
		{
			CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
		}

		/// <inheritdoc />
		public override byte[] PackRequest()
		{
            byte[] req = new byte[12];
            ModbusWriteCommandParameters mbParams = (ModbusWriteCommandParameters)CommandParameters;

            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)mbParams.TransactionId)), 0, req, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)mbParams.ProtocolId)), 0, req, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)mbParams.Length)), 0, req, 4, 2);
            req[6] = mbParams.UnitId;
            req[7] = mbParams.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)mbParams.OutputAddress)), 0, req, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Htons((short)mbParams.Value)), 0, req, 10, 2);

            return req;
		}

		/// <inheritdoc />
		public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
		{
            var retval = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusWriteCommandParameters mbParams = (ModbusWriteCommandParameters)CommandParameters;

            retval.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, mbParams.OutputAddress), mbParams.Value);

            return retval;
		}
	}
}