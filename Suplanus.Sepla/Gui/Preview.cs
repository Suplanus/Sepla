﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Eplan.EplApi.DataModel;
using System.Runtime.InteropServices;
using Eplan.EplApi.DataModel.MasterData;
using Eplan.EplApi.HEServices;

namespace Suplanus.Sepla.Gui
{
	public class Preview
	{
		private readonly Border _border;
		private readonly DrawingService _drawingService;
		private readonly Project _project;

		public Preview(Border border, string projectFile)
		{
			var projectManager = new ProjectManager();
			projectManager.LockProjectByDefault = false;
			_project = projectManager.OpenProject(projectFile, ProjectManager.OpenMode.Exclusive);

			_drawingService = new DrawingService();
			_drawingService.DrawConnections = true;

			_border = border;
		}

		public void Display(string path, PreviewType previewType)
		{
			switch (previewType)
			{
				case PreviewType.WindowMacro:
					WindowMacro windowMacro = new WindowMacro();
					windowMacro.Open(path, _project);
					_drawingService.CreateDisplayList(windowMacro);
					DrawEplan();
					windowMacro.Dispose();
					break;

				case PreviewType.SymbolMacro:
					SymbolMacro symbolMacro = new SymbolMacro();
					symbolMacro.Open(path, _project);
					_drawingService.CreateDisplayList(symbolMacro);
					DrawEplan();
					symbolMacro.Dispose();
					break;

				case PreviewType.PageMacro:
					PageMacro pageMacro = new PageMacro();
					pageMacro.Open(path, _project);
					_drawingService.CreateDisplayList(pageMacro.Pages);
					DrawEplan();
					pageMacro.Dispose();
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(previewType), previewType, null);
			}

		}

		private void DrawEplan()
		{
			int width = Convert.ToInt16(_border.ActualWidth);
			int height = Convert.ToInt16(_border.ActualHeight);

			if (width > 0 && height > 0)
			{
				System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height);
				System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);
				System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(0, 0, width, height);
				PaintEventArgs paintEventArgs = new PaintEventArgs(graphics, rectangle);

				_drawingService.DrawDisplayList(paintEventArgs);

				IntPtr hBitmap = bitmap.GetHbitmap();
				BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
					IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

				_border.Background = new ImageBrush(bitmapSource);

				bitmap.Dispose();
				graphics.Dispose();
				paintEventArgs.Dispose();
				DeleteObject(hBitmap);
			}
			else
			{
				_border.Background = null;
			}

		}

		/// <summary>
		/// Memory Leak: http://stackoverflow.com/questions/1546091/wpf-createbitmapsourcefromhbitmap-memory-leak
		/// </summary>
		/// <param name="hObject"></param>
		/// <returns></returns>
		[DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);
	}

	

	public enum PreviewType
	{
		WindowMacro,
		SymbolMacro,
		PageMacro
	}
}
