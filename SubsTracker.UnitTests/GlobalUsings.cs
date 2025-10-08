//System
global using System;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Linq.Expressions;

//Packages
global using AutoMapper;
global using Xunit;
global using NSubstitute;
global using Shouldly;
global using AutoFixture;
global using AutoFixture.Xunit2;

//DAL
global using SubsTracker.DAL.Models.Subscription;
global using SubsTracker.DAL.Models.User;
global using SubsTracker.DAL.Interfaces.Repositories;

//BLL
global using SubsTracker.BLL.DTOs.User;
global using SubsTracker.BLL.DTOs.User.Create;
global using SubsTracker.BLL.DTOs.User.Update;
global using SubsTracker.BLL.DTOs.Subscription;
global using SubsTracker.BLL.Interfaces;
global using SubsTracker.BLL.Interfaces.User;
global using SubsTracker.BLL.Services;

//Domain
global using SubsTracker.Domain.Filter;
global using SubsTracker.Domain.Enums;
global using SubsTracker.Domain.Exceptions;
global using InvalidOperationException = SubsTracker.Domain.Exceptions.InvalidOperationException;
