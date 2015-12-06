﻿using System;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using System.Diagnostics;
using Efalg5GeometrischeAlgo;
using System.Diagnostics.Contracts;

namespace U4LongLiveTheSquare
{
	/// <summary>
	/// Your application's main form
	/// </summary>
	public class MainForm : Form
	{
		GridView canvas;

		bool drag = false;

		public MainForm ()
		{
			Title = "Long live the square!";
			ClientSize = new Size (500, 500);

			canvas = new GridView ();
			canvas.LoadComplete += (sender, e) => canvas.Update ();
			canvas.MouseDown += Canvas_MouseDown;
			canvas.MouseUp += Canvas_MouseUp;
			canvas.MouseMove += Canvas_MouseMove;
			canvas.MouseWheel += Canvas_MouseWheel;

			Content = canvas;

			var calcConvexHull = new Command {
				MenuText = "Convex Hull",
				ToolBarText = "Convex Hull",
				Image = Bitmap.FromResource ("U4LongLiveTheSquare.Images.convex_hull.fw.png")
			};
			calcConvexHull.Executed += CalcConvexHull_Executed;

			var calcBoundingBox = new Command {
				MenuText = "Bounding Box",
				ToolBarText = "Bounding Box",
				Image = Bitmap.FromResource ("U4LongLiveTheSquare.Images.bounding_box.fw.png")
			};
			calcBoundingBox.Executed += CalcBoundingBox_Executed;

			var resetGrid = new Command {
				MenuText = "Reset",
				ToolBarText = "Reset",
				Image = Bitmap.FromResource ("U4LongLiveTheSquare.Images.reset.fw.png")
			};
			resetGrid.Executed += ResetGrid_Executed;

			var randomPoints = new Command {
				MenuText = "Random",
				ToolBarText = "Random",
				Image = Bitmap.FromResource ("U4LongLiveTheSquare.Images.random.fw.png")
			};
			randomPoints.Executed += RandomPoints_Executed;
			;

			var quitCommand = new Command {
				MenuText = "Quit",
				Shortcut = Application.Instance.CommonModifier | Keys.Q
			};
			quitCommand.Executed += (sender, e) => Application.Instance.Quit ();

			var aboutCommand = new Command { MenuText = "About" };
			aboutCommand.Executed += (sender, e) => MessageBox.Show (this, "developed by Florian Bruggisser 2015");

			// create menu
			Menu = new MenuBar {
				Items = {
					// File submenu
					new ButtonMenuItem { Text = "&File", Items = { calcConvexHull } },
				},
				ApplicationItems = {
					// application (OS X) or file menu (others)
					new ButtonMenuItem { Text = "&Preferences..." },
				},
				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};

			// create toolbar			
			ToolBar = new ToolBar { Items = { resetGrid, randomPoints, calcConvexHull, calcBoundingBox } };
		}

		void CalcBoundingBox_Executed (object sender, EventArgs e)
		{
			//clear old lines
			var oldBoxes = canvas.Geometries.OfType<Polygon2d> ().ToArray ();
			foreach (var b in oldBoxes)
				canvas.Geometries.Remove (b);
			
			//calculate new box
			var box = MinimalBoundingBox.Calculate (canvas.Geometries.OfType<Vector2d> ().ToArray ());
			canvas.Geometries.Add (box);
			canvas.Update ();
		}

		void RandomPoints_Executed (object sender, EventArgs e)
		{
			var r = new Random ();
			for (var i = 0; i < 20; i++) {
				AddPoint (r.Next (0, canvas.Width - 20), r.Next (0, canvas.Height - 20));
			}
			canvas.Update ();
		}

		void ResetGrid_Executed (object sender, EventArgs e)
		{
			canvas.Geometries.Clear ();
			canvas.ScaleFactor = 8;
			canvas.Update ();
		}

		void Canvas_MouseWheel (object sender, MouseEventArgs e)
		{
			//todo: implement smooth scrolling
			var direction = 0 > e.Delta.Height ? -1 : 1;
			canvas.ScaleFactor += Math.Min (0.1f, Math.Abs (e.Delta.Height)) * direction;
			canvas.ScaleFactor = Math.Max (0, canvas.ScaleFactor);
			canvas.Update ();
		}

		void Canvas_MouseDown (object sender, MouseEventArgs e)
		{
			drag = e.Modifiers == Keys.Alt;
		}

		void Canvas_MouseMove (object sender, MouseEventArgs e)
		{
			if (e.Modifiers == Keys.Alt) {
				Cursor = Cursors.Move;
				if (drag) {
					Debug.WriteLine ("dragged: " + e);
					canvas.TransformationDelta.Offset (20, 0);
				}
			} else {
				Cursor = Cursors.Crosshair;
			}
		}

		void Canvas_MouseUp (object sender, MouseEventArgs e)
		{
			if (!drag) {
				AddPoint (e.Location.X, e.Location.Y);
				canvas.Update ();
			}

			drag = false;
		}

		void CalcConvexHull_Executed (object sender, EventArgs e)
		{
			//clear old lines
			var oldHull = canvas.Geometries.OfType<Segment2d> ().ToArray ();
			foreach (var l in oldHull)
				canvas.Geometries.Remove (l);

			//get all points
			var points = canvas.Geometries.OfType<Vector2d> ().ToArray ();
			var convexHull = GeoAlgos.MonotoneChainConvexHull (points);

			//draw convex hull
			for (var i = 0; i < convexHull.Length; i++) {
				var next = (i + 1) % convexHull.Length;
				canvas.Geometries.Add (new Segment2d (convexHull [i], convexHull [next]));
			}

			canvas.Update ();
		}

		void AddPoint (float x, float y)
		{
			var m = canvas.ProjectionMatrix;
			var translatedPoint = new PointF ((x - m.X0) / m.Xx, (y - m.Y0) / m.Yy);

			canvas.Geometries.Add (new Vector2d (translatedPoint.X, translatedPoint.Y));
		}
	}
}
