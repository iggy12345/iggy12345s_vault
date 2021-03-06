﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using FireworkToolkit.Templates;

namespace FireworkToolkit.Interfaces
{
    /// <summary>
    /// Interface to represent a particle in the fireworks system
    /// </summary>
    public interface IParticle
    {
        /// <summary>
        /// Steps the particle through n seconds of time
        /// </summary>
        /// <param name="steps">The number of steps/seconds to occur</param>
        void Update(int steps = 1);

        /// <summary>
        /// Applies a force to a particle in the given magnitude and direction
        /// </summary>
        /// <param name="force">Force vector to apply</param>
        void ApplyForce(AVector force);

        /// <summary>
        /// Draws the particle onto the screen using the graphics object specified
        /// </summary>
        /// <param name="g">The graphics object to use</param>
        void Show(System.Drawing.Graphics g);

        /// <summary>
        /// Draws the particle onto the image supplied
        /// </summary>
        /// <param name="img">The image to draw on</param>
        void Show(Bitmap img);

        /// <summary>
        /// Clones the particle
        /// </summary>
        /// <returns>Returns a new particle object with the same attributes as this one</returns>
        IParticle Clone();
    }
}
