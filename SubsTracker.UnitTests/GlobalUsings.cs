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

//DAL
global using SubsTracker.DAL.Models.Subscription;
global using SubsTracker.DAL.Models.User;
global using SubsTracker.DAL.Interfaces.Repositories;

//BLL
global using SubsTracker.BLL.DTOs.User;
global using SubsTracker.BLL.DTOs.User.Create;
global using SubsTracker.BLL.DTOs.User.Update;
global using SubsTracker.BLL.DTOs.Subscription;
global using SubsTracker.BLL.Interfaces.User;
global using MemberModelService = SubsTracker.BLL.Services.User.GroupMemberService;
global using SubscriptionModelService = SubsTracker.BLL.Services.Subscription.SubscriptionService;
global using UserModelService = SubsTracker.BLL.Services.User.UserService;
global using GroupModelService = SubsTracker.BLL.Services.User.UserGroupService;
global using SubsTracker.BLL.Interfaces.Cache;
global using SubsTracker.BLL.RedisSettings;

//Domain
global using SubsTracker.Domain.Filter;
global using SubsTracker.Domain.Enums;
global using SubsTracker.Domain.Exceptions;
global using InvalidOperationException = SubsTracker.Domain.Exceptions.InvalidOperationException;

//Messaging
global using SubsTracker.Messaging.Interfaces;
global using SubsTracker.Messaging.Contracts;
