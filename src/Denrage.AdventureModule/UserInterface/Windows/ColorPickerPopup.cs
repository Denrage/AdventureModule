using Blish_HUD;
using Blish_HUD._Extensions;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AdventureModule.UserInterface.Windows
{
    public class ColorPickerPopup : WindowBase2
    {
        public void Initialize(ColorBox associatedColorBox)
        {
            this.ConstructWindow(ContentService.Textures.TransparentPixel, new Rectangle(0, 0, 500, 500), new Rectangle(0, 40, 500, 500 - 40));
            this.Location = new Point(0, 100);

            var colorPicker = new ColorPicker()
            {
                Parent = this,
                Width = this.ContentRegion.Width,
                Height = this.ContentRegion.Height,
            };

            Task.Run(async () =>
            {
                var colors = await Module.Instance.Gw2ApiManager.Gw2ApiClient.V2.Colors.AllAsync();
                var groupedColors = colors.Where(x => x.Categories.Any()).GroupBy(x => x.Categories[0]);
                foreach (var item in groupedColors.Select(x => x.Take(10)).SelectMany(x => x))
                {

                    colorPicker.Colors.Add(item);
                }

                if (associatedColorBox.Color == null)
                {
                    associatedColorBox.Color = colorPicker.Colors.FirstOrDefault();
                }

                colorPicker.AssociatedColorBox = associatedColorBox;

                associatedColorBox.ColorChanged += (s, e) =>
                {
                    this.Dispose();
                };
            });

        }
    }
}
