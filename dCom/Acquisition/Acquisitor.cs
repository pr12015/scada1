using dCom.Configuration;
using dCom.Connection;
using dCom.ViewModel;
using System;
using System.Threading;
using dCom.Modbus.ModbusFunctions;
using dCom.Modbus.FunctionParameters;
using dCom.Modbus;

namespace dCom.Acquisition
{
	public class Acquisitor : IDisposable
	{
		private AutoResetEvent acquisitionTrigger;
		private FunctionExecutor commandExecutor;
		private Thread acquisitionWorker;
		private IStateUpdater stateUpdater;
		private bool acquisitionStopSignal = true;

		public Acquisitor(AutoResetEvent acquisitionTrigger, FunctionExecutor commandExecutor, IStateUpdater stateUpdater)
		{
			this.stateUpdater = stateUpdater;
			this.acquisitionTrigger = acquisitionTrigger;
			this.commandExecutor = commandExecutor;
			this.InitializeAcquisitionThread();
			this.StartAcquisitionThread();
		}

		#region Private Methods

		private void InitializeAcquisitionThread()
		{
			this.acquisitionWorker = new Thread(Acquisition_DoWork);
			this.acquisitionWorker.Name = "Acquisition thread";
		}

		private void StartAcquisitionThread()
		{
			acquisitionWorker.Start();
		}

		/// <summary>
		/// Acquisition thread
		///		Awaits for trigger;
		///		After configured period send appropriate command to MdbSim for each point type
		/// 
		///		Kao uslov za while petlju korititi acquisitionStopSignal da bi se akvizicioni thread ugasio kada se aplikacija ugasi
		/// </summary>
		private void Acquisition_DoWork()
		{
			try
			{
                ConfigReader reader = ConfigReader.Instance;
                int AO, AO1, AI, AI1, DO, DO1, DI, DI1;
                AO = AO1 = reader.GetAcquisitionInterval("AnaOut");
                AI = AI1 = reader.GetAcquisitionInterval("AnaIn");
                DO = DO1 = reader.GetAcquisitionInterval("DigOut");
                DI = DI1 = reader.GetAcquisitionInterval("DigIn");

                int cnt = 0;
                ModbusReadCommandParameters mbParams = null;
                ModbusFunction fn = null;

                while (acquisitionStopSignal)
                {
                    if(cnt % AO == 0)
                    {
                        mbParams = new ModbusReadCommandParameters(6, (byte)ModbusFunctionCode.READ_HOLDING_REGISTERS, reader.GetStartAddress("AnaOut"), reader.GetNumberOfRegisters("AnaOut"));
                        fn = FunctionFactory.CreateModbusFunction(mbParams);
                        commandExecutor.EnqueueCommand(fn);
                    }
                    if(cnt % AO1 == 0)
                    {
                        mbParams = new ModbusReadCommandParameters(6, (byte)ModbusFunctionCode.READ_HOLDING_REGISTERS, reader.GetStartAddress("AnaOut1"), reader.GetNumberOfRegisters("AnaOut1"));
                        fn = FunctionFactory.CreateModbusFunction(mbParams);
                        commandExecutor.EnqueueCommand(fn);
                    }
                    if(cnt % AI == 0)
                    {
                        mbParams = new ModbusReadCommandParameters(6, (byte)ModbusFunctionCode.READ_INPUT_REGISTERS, reader.GetStartAddress("AnaIn"), reader.GetNumberOfRegisters("AnaIn"));
                        fn = FunctionFactory.CreateModbusFunction(mbParams);
                        commandExecutor.EnqueueCommand(fn);
                    }
                    if(cnt % AI1 == 0)
                    {
                        mbParams = new ModbusReadCommandParameters(6, (byte)ModbusFunctionCode.READ_INPUT_REGISTERS, reader.GetStartAddress("AnaIn1"), reader.GetNumberOfRegisters("AnaIn1"));
                        fn = FunctionFactory.CreateModbusFunction(mbParams);
                        commandExecutor.EnqueueCommand(fn);
                    }
                    if (cnt % DO == 0)
                    {
                        mbParams = new ModbusReadCommandParameters(6, (byte)ModbusFunctionCode.READ_COILS, reader.GetStartAddress("DigOut"), reader.GetNumberOfRegisters("DigOut"));
                        fn = FunctionFactory.CreateModbusFunction(mbParams);
                        commandExecutor.EnqueueCommand(fn);
                    }
                    if (cnt % DO1 == 0)
                    {
                        mbParams = new ModbusReadCommandParameters(6, (byte)ModbusFunctionCode.READ_COILS, reader.GetStartAddress("DigOut1"), reader.GetNumberOfRegisters("DigOut1"));
                        fn = FunctionFactory.CreateModbusFunction(mbParams);
                        commandExecutor.EnqueueCommand(fn);
                    }
                    if (cnt % DI == 0)
                    {
                        mbParams = new ModbusReadCommandParameters(6, (byte)ModbusFunctionCode.READ_DISCRETE_INPUTS, reader.GetStartAddress("DigIn"), reader.GetNumberOfRegisters("DigIn"));
                        fn = FunctionFactory.CreateModbusFunction(mbParams);
                        commandExecutor.EnqueueCommand(fn);
                    }
                    if (cnt % DI1 == 0)
                    {
                        mbParams = new ModbusReadCommandParameters(6, (byte)ModbusFunctionCode.READ_DISCRETE_INPUTS, reader.GetStartAddress("DigIn1"), reader.GetNumberOfRegisters("DigIn1"));
                        fn = FunctionFactory.CreateModbusFunction(mbParams);
                        commandExecutor.EnqueueCommand(fn);
                    }

                    try
                    {
                        ++cnt;
                        acquisitionTrigger.WaitOne();
                    }
                    catch (Exception ex)
                    {
                        string message = $"{ex.TargetSite.ReflectedType.Name}.{ex.TargetSite.Name}: {ex.Message}";
                        stateUpdater.LogMessage(message);
                    }
                } // end while


			}
			catch (Exception ex)
			{
				string message = $"{ex.TargetSite.ReflectedType.Name}.{ex.TargetSite.Name}: {ex.Message}";
				stateUpdater.LogMessage(message);
			}
		}

		#endregion Private Methods

		public void Dispose()
		{
			acquisitionStopSignal = false;
		}
	}
}