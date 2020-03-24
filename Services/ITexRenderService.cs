using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace BoschBot.Services
{
    public interface ITexRenderService
    {
        Task<Image> RenderExpression(string expression);
    }
}
