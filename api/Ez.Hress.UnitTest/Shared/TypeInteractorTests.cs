﻿using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Shared;

public class TypeInteractorTests
{
    private readonly Mock<ITypeDataAccess> _typeDataAccessMock;
    private readonly Mock<ILogger<TypeInteractor>> _typeLog;

    public TypeInteractorTests()
    {
        _typeDataAccessMock = new(MockBehavior.Strict);
        _typeLog = new();
    }

    [Fact]
    public async Task GetTypesOK_TestsAsync()
    {
        // ARRANGE
        IList<TypeEntity> list = new List<TypeEntity>();
        _typeDataAccessMock.Setup(t => t.GetTypes()).Returns(Task.FromResult<IList<TypeEntity>>(list));
        var interactor = new TypeInteractor(_typeDataAccessMock.Object, _typeLog.Object);

        // ACT
        var result = await interactor.GetEzTypes();

        // ASSERT
        Assert.NotNull(result);
        _typeDataAccessMock.Verify();
    }
}
