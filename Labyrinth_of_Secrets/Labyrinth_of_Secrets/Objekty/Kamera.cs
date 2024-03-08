using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labyrinth_of_Secrets
{
    public class Kamera
    {
        private readonly Viewport _viewport;

        public Kamera(Viewport viewport, float zoom)
        {
            _viewport = viewport;
            this.zoom = zoom;
            origin = new Vector2(viewport.Width / 2f, viewport.Height / 2f);
        }

        public Vector2 pozice { get; set; }
        public float zoom { get; set; }
        public Vector2 origin { get; set; }

        public Matrix GetViewMatrix()
        {
            return
                Matrix.CreateTranslation(new Vector3(-pozice, 0.0f)) *
                Matrix.CreateTranslation(new Vector3(-origin, 0.0f)) *
                Matrix.CreateScale(zoom, zoom, 1) *
                Matrix.CreateTranslation(new Vector3(origin, 0.0f));
        }

        public static Matrix GetViewMatrix(Vector2 pozice, Vector2 origin, float zoom)
        {
            return
                Matrix.CreateTranslation(new Vector3(-pozice, 0.0f)) *
                Matrix.CreateTranslation(new Vector3(-origin, 0.0f)) *
                Matrix.CreateScale(zoom, zoom, 1) *
                Matrix.CreateTranslation(new Vector3(origin, 0.0f));
        }
    }
}
