﻿using System;
using System.Linq;
using RouteManager2;
using RouteManager2.Events;

namespace CsvRwRouteParser
{
	/// <summary>A legacy Brightness command</summary>
	/// <remarks>Applies equally to all tracks in a block</remarks>
	internal class Brightness
	{
		/// <summary>The track position for the brightness change</summary>
		internal readonly double TrackPosition;

		/// <summary>The new brightness value</summary>
		internal readonly float Value;

		internal Brightness(double trackPosition, float value)
		{
			TrackPosition = trackPosition;
			Value = value;
		}

		internal void Create(CurrentRoute CurrentRoute, double StartingDistance, int CurrentElement, ref int CurrentBrightnessElement, ref int CurrentBrightnessEvent, ref double CurrentBrightnessTrackPosition, ref float CurrentBrightnessValue)
		{
			for (int tt = 0; tt < CurrentRoute.Tracks.Count; tt++)
			{
				int t = CurrentRoute.Tracks.ElementAt(tt).Key;
				int m = CurrentRoute.Tracks[t].Elements[CurrentElement].Events.Length;
				Array.Resize(ref CurrentRoute.Tracks[t].Elements[CurrentElement].Events, m + 1);
				double d = TrackPosition - StartingDistance;
				CurrentRoute.Tracks[t].Elements[CurrentElement].Events[m] = new BrightnessChangeEvent(d, Value, CurrentBrightnessValue, TrackPosition - CurrentBrightnessTrackPosition);

				if (t == 0)
				{
					if (CurrentBrightnessElement >= 0 & CurrentBrightnessEvent >= 0)
					{
						BrightnessChangeEvent bce = (BrightnessChangeEvent) CurrentRoute.Tracks[t].Elements[CurrentBrightnessElement].Events[CurrentBrightnessEvent];
						bce.NextBrightness = Value;
						bce.NextDistance = TrackPosition - CurrentBrightnessTrackPosition;
					}

					CurrentBrightnessEvent = m;

				}
				else
				{
					if (CurrentBrightnessElement >= 0 & CurrentBrightnessEvent >= 0)
					{
						for (int e = 0; e < CurrentRoute.Tracks[t].Elements[CurrentBrightnessElement].Events.Length; e++)
						{
							if (!(CurrentRoute.Tracks[t].Elements[CurrentBrightnessElement].Events[e] is BrightnessChangeEvent))
								continue;
							BrightnessChangeEvent bce = (BrightnessChangeEvent) CurrentRoute.Tracks[t].Elements[CurrentBrightnessElement].Events[e];
							bce.NextBrightness = Value;
							bce.NextDistance = TrackPosition - CurrentBrightnessTrackPosition;
						}
					}
				}
			}
			CurrentBrightnessElement = CurrentElement;
			CurrentBrightnessTrackPosition = TrackPosition;
			CurrentBrightnessValue = Value;
		}
	}
}
