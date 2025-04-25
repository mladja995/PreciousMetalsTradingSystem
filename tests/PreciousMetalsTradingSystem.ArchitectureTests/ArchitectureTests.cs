using FluentAssertions;
using PreciousMetalsTradingSystem.Application.Database;
using MediatR;
using NetArchTest.Rules;

namespace PreciousMetalsTradingSystem.ArchitectureTests
{
    public class ArhitectureTests
    {
        private const string DomainNamespace = "PreciousMetalsTradingSystem.Domain";
        private const string ApplicationNamespace = "PreciousMetalsTradingSystem.Application";
        private const string InfrastructureNamespace = "PreciousMetalsTradingSystem.Infrastructure";
        private const string WebNamespace = "PreciousMetalsTradingSystem.WebApi";

        [Fact]
        public void Domain_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange
            var assembly = typeof(Domain.AssemblyReference).Assembly;

            var otherProjects = new[]
            {
                ApplicationNamespace,
                InfrastructureNamespace,
                WebNamespace
            };

            // Act
            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAll(otherProjects)
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Application_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange
            var assembly = typeof(Application.AssemblyReference).Assembly;

            var otherProjects = new[]
            {
                InfrastructureNamespace,
                WebNamespace
            };

            // Act
            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAll(otherProjects)
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Infrastructure_Should_Not_HaveDependencyOnOtherProjects()
        {
            // Arrange
            var assembly = typeof(Infrastructure.AssemblyReference).Assembly;

            var otherProjects = new[]
            {
                WebNamespace
            };

            // Act
            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAll(otherProjects)
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Controllers_Should_HaveDependencyOnMediatR()
        {
            // Arrange
            var assembly = typeof(WebApi.AssemblyReference).Assembly;

            // Act
            var testResult = Types
                .InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Controller")
                .Should()
                .HaveDependencyOn("MediatR")
                .GetResult();

            // Assert
            testResult.IsSuccessful.Should().BeTrue();
        }

        //[Fact]
        //public void Handlers_Should_Have_DependencyOnDomain()
        //{
        //    // Arrange
        //    var assembly = typeof(Application.AssemblyReference).Assembly;

        //    // Act
        //    var testResult = Types
        //        .InAssembly(assembly)
        //        .That()
        //        .HaveNameEndingWith("Handler")
        //        .Should()
        //        .HaveDependencyOn(DomainNamespace)
        //        .GetResult();

        //    // Assert
        //    testResult.IsSuccessful.Should().BeTrue();
        //}

        [Fact]
        public void Handlers_Should_Not_HaveDependencyOnInfrastructureOrWebApi()
        {
            // Arrange
            var assembly = typeof(Application.AssemblyReference).Assembly;

            var forbiddenDependencies = new[]
            {
                InfrastructureNamespace,
                WebNamespace
            };

            // Act
            var result = Types
                .InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Handler")
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenDependencies)
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void Controllers_Should_BeProperlyNamedAndLocated()
        {
            // Arrange
            var assembly = typeof(WebApi.AssemblyReference).Assembly;

            // Act
            var result = Types
                .InAssembly(assembly)
                .That()
                .ResideInNamespace("PreciousMetalsTradingSystem.WebApi.Controllers")
                .Should()
                .HaveNameEndingWith("Controller")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void IRequest_Implementations_Should_EndWithCommandOrQuery()
        {
            // Arrange
            var assembly = typeof(Application.AssemblyReference).Assembly;
            var requestType = typeof(IRequest<>);

            // Act
            var result = Types
                .InAssembly(assembly)
                .That()
                .ImplementInterface(requestType)
                .Should()
                .HaveNameEndingWith("Command")
                .Or()
                .HaveNameEndingWith("Query")
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void WebApi_Should_Not_Use_IUnitOfWork_Or_IRepository()
        {
            // Arrange
            var assembly = typeof(WebApi.AssemblyReference).Assembly;
            var forbiddenTypes = new[]
            {
                typeof(IUnitOfWork),
                typeof(IRepository<,>)
            };

            // Act
            var result = Types
                .InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenTypes.Select(t => t.Namespace).ToArray())
                .GetResult();

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
    }
}
