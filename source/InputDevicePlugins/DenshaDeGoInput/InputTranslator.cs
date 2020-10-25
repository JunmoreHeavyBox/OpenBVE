﻿//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Marc Riera, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using OpenTK.Input;

namespace DenshaDeGoInput
{
	/// <summary>
	/// Class which holds generic controller input.
	/// </summary>
	internal static class InputTranslator
	{
		/// <summary>
		/// Enumeration representing controller models.
		/// </summary>
		internal enum ControllerModels
		{
			Unsupported = 0,
			Classic = 1,
			Unbalance = 2,
		};

		/// <summary>
		/// Enumeration representing brake notches.
		/// </summary>
		internal enum BrakeNotches
		{
			Released = 0,
			B1 = 1,
			B2 = 2,
			B3 = 3,
			B4 = 4,
			B5 = 5,
			B6 = 6,
			B7 = 7,
			B8 = 8,
			Emergency = 9,
		};

		/// <summary>
		/// Enumeration representing power notches.
		/// </summary>
		internal enum PowerNotches
		{
			N = 0,
			P1 = 1,
			P2 = 2,
			P3 = 3,
			P4 = 4,
			P5 = 5,
		};

		/// <summary>
		/// Class with the state of the buttons.
		/// </summary>
		internal class ButtonState
		{
			internal OpenTK.Input.ButtonState Select;
			internal OpenTK.Input.ButtonState Start;
			internal OpenTK.Input.ButtonState A;
			internal OpenTK.Input.ButtonState B;
			internal OpenTK.Input.ButtonState C;
			internal OpenTK.Input.ButtonState D;
			internal OpenTK.Input.ButtonState Up;
			internal OpenTK.Input.ButtonState Down;
			internal OpenTK.Input.ButtonState Left;
			internal OpenTK.Input.ButtonState Right;
		}

		/// <summary>
		/// The index of the active controller, or -1 if not set.
		/// </summary>
		public static int activeControllerIndex = -1;

		/// <summary>
		/// Whether a supported controller is connected or not.
		/// </summary>
		internal static bool IsControllerConnected;

		/// <summary>
		/// The controller model.
		/// </summary>
		internal static ControllerModels ControllerModel;

		/// <summary>
		/// The current brake notch reported by the controller.
		/// </summary>
		internal static BrakeNotches BrakeNotch;

		/// <summary>
		/// The current power notch reported by the controller.
		/// </summary>
		internal static PowerNotches PowerNotch;

		/// <summary>
		/// The previous brake notch reported by the controller.
		/// </summary>
		internal static BrakeNotches PreviousBrakeNotch;

		/// <summary>
		/// The previous power notch reported by the controller.
		/// </summary>
		internal static PowerNotches PreviousPowerNotch;

		/// <summary>
		/// The state of the controller's buttons.
		/// </summary>
		internal static ButtonState ControllerButtons = new ButtonState();

		/// <summary>
		/// Gets the controller model.
		/// </summary>
		internal static ControllerModels GetControllerModel(JoystickState state, JoystickCapabilities capabilities)
		{
			if (ControllerUnbalance.IsCompatibleController(capabilities))
			{
				return ControllerModels.Unbalance;
			}
			if (ControllerClassic.IsCompatibleController(capabilities))
			{
				return ControllerModels.Classic;
			}
			return ControllerModels.Unsupported;
		}

		/// <summary>
		/// Updates the status of the controller.
		/// </summary>
		public static void Update()
		{
			if (!IsControllerConnected)
			{
				// The controller is apparently not connected; try to connect to it
				JoystickState state = Joystick.GetState(activeControllerIndex);
				JoystickCapabilities capabilities = Joystick.GetCapabilities(activeControllerIndex);
				ControllerModel = GetControllerModel(state, capabilities);

				// HACK: IsConnected seems to be broken on Mono, so we use the button count instead
				if (capabilities.ButtonCount > 0 && ControllerModel != ControllerModels.Unsupported)
				{
					// The controller is valid and can be used
					IsControllerConnected = true;
					return;
				}
				// The controller is not valid; temporarily discard input from the device
				activeControllerIndex = -1;
			}
			else
			{
				// HACK: IsConnected seems to be broken on Mono, so we use the button count instead
				if (Joystick.GetCapabilities(activeControllerIndex).ButtonCount == 0)
				{
					// The controller is apparently not connected
					IsControllerConnected = false;
					return;
				}
				GetInput();
			}
		}

		/// <summary>
		/// Gets the input from the controller.
		/// </summary>
		internal static void GetInput()
		{
			// Store the current notches so we can later tell if they have been moved
			PreviousBrakeNotch = BrakeNotch;
			PreviousPowerNotch = PowerNotch;

			// Read the input from the controller according to the type
			switch (ControllerModel)
			{
				case ControllerModels.Classic:
					ControllerClassic.ReadInput(Joystick.GetState(activeControllerIndex));
					return;
				case ControllerModels.Unbalance:
					ControllerUnbalance.ReadInput(Joystick.GetState(activeControllerIndex));
					return;
			}
		}

		/// <summary>
		/// Gets the state of the buttons of the current controller
		/// </summary>
		/// <returns>State of the buttons of the current controller</returns>
		internal static List<OpenTK.Input.ButtonState> GetButtonsState()
		{
			List<OpenTK.Input.ButtonState> buttonsState = new List<OpenTK.Input.ButtonState>();

			if (IsControllerConnected)
			{
				for (int i = 0; i < Joystick.GetCapabilities(activeControllerIndex).ButtonCount; i++)
				{
					buttonsState.Add(Joystick.GetState(activeControllerIndex).GetButton(i));
				}
			}
			return buttonsState;
		}

		/// <summary>
		/// Gets the position of the hats of the current controller
		/// </summary>
		/// <returns>Position of the hats of the current controller</returns>
		internal static List<HatPosition> GetHatPositions()
		{
			List<HatPosition> hatPositions = new List<HatPosition>();

			if (IsControllerConnected)
			{
				for (int i = 0; i < Joystick.GetCapabilities(activeControllerIndex).ButtonCount; i++)
				{
					hatPositions.Add(Joystick.GetState(activeControllerIndex).GetHat((JoystickHat)i).Position);
				}
			}
			return hatPositions;
		}

		/// <summary>
		/// Compares two button states to find the index of the button that has been pressed.
		/// </summary>
		/// <returns>Index of the button that has been pressed.</returns>
		internal static int GetDifferentPressedIndex(List<OpenTK.Input.ButtonState> previousState, List<OpenTK.Input.ButtonState> newState, List<int> ignored)
		{
			for (int i = 0; i < newState.Count; i++)
			{
				if (!ignored.Contains(i) && newState[i] != previousState[i])
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Compares two hat states to find the index of the hat that has changed.
		/// </summary>
		/// <returns>Index of the hat that has changed.</returns>
		internal static int GetChangedHat(List<HatPosition> previousPosition, List<HatPosition> newPosition)
		{
			for (int i = 0; i < newPosition.Count; i++)
			{
				if (newPosition[i] != previousPosition[i])
					return i;
			}
			return -1;
		}

	}
}
