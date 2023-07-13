﻿using Backend.Models;
namespace Backend.Repositories
{
    public interface IModuleRepository
    {

        Task<bool> ModuleExistsAsync(string moduleName);
        Task AddModuleAsync(Module module);
    }
}