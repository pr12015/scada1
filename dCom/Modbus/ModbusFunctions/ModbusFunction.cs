﻿using dCom.Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace dCom.Modbus.ModbusFunctions
{
	public abstract class ModbusFunction
	{
		private ModbusCommandParameters commandParameters;

		public ModbusFunction(ModbusCommandParameters commandParameters)
		{
			this.commandParameters = commandParameters;
		}

		public ModbusCommandParameters CommandParameters
		{
			get
			{
				return commandParameters;
			}

			set
			{
				commandParameters = value;
			}
		}

		public override string ToString()
		{
			return $"Transaction: {commandParameters.TransactionId}, command {commandParameters.FunctionCode}";
		}

		protected void CheckArguments(MethodBase m, Type t)
		{
			if (commandParameters.GetType() != t)
			{
				string message = $"{m.ReflectedType.Name}{m.Name} has invalid argument {nameof(commandParameters)} of type {commandParameters.GetType().Name}.{Environment.NewLine}Argumet type should be {t.Name}";
				throw new ArgumentException(message);
			}
		}

        /// <summary>
        /// Converts to network order (big endian)
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        protected short Htons(short host)
        {
            return IPAddress.HostToNetworkOrder(host);
        }

        /// <summary>
        /// Method is called from communication thread:
        /// Converts command parameters to byte array
        /// Parameters should be packed according to this:
        ///
        ///
        /// </summary>
        /// <returns>Command parameters in form of byte array</returns>
        public abstract byte[] PackRequest();

		/// <summary>
		/// Method is called from communication thread:
		/// Converts received message to key-value pairs
		/// Response is packed according to this:
		///
		///
		/// </summary>
		/// <param name="response">Message read form socket</param>
		/// <returns>
		///		Dictionary that maps tuple to received value from MdbSim:
		///		Key: Tuple<PointType, ushort> - complex key of point. Points unique identifier
		///				- PointType - type of point
		///				- Point address
		///		Value: Value received from MdbSim
		/// </returns>
		public abstract Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response);
	}
}