using System;

using SoRaVAC.Models;

namespace SoRaVAC.Contracts.Services
{
    public interface IThemeSelectorService
    {
        void InitializeTheme();

        void SetTheme(AppTheme theme);

        AppTheme GetCurrentTheme();
    }
}
