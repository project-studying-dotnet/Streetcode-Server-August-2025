using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Streetcode.Update;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Realizations.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.Update
{
    public class UpdateStreecodeHandler : IRequestHandler<UpdateStreetcodeCommand, Result<StreetcodeUpdateDTO>>
    {
        private IRepositoryWrapper _repositoryWrapper;
        private IMapper _mapper;
        private ILoggerService _logger;

        public UpdateStreecodeHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public Task<Result<StreetcodeUpdateDTO>> Handle(UpdateStreetcodeCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
