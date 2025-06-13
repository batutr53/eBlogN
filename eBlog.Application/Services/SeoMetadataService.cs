using AutoMapper;
using eBlog.Application.DTOs;
using eBlog.Application.Interfaces;
using eBlog.Domain.Entities;
using eBlog.Domain.Interfaces;
using eBlog.Shared.Results;

namespace eBlog.Application.Services
{
    public class SeoMetadataService : GenericService<SeoMetadata, SeoMetadataDto, SeoMetadataCreateDto, SeoMetadataUpdateDto>, ISeoMetadataService
    {
        private readonly IGenericRepository<SeoMetadata> _repository;
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;

        public SeoMetadataService(
            IGenericRepository<SeoMetadata> repository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IPostRepository postRepository
        ) : base(repository, unitOfWork, mapper)
        {
            _repository = repository;
            _mapper = mapper;
            _postRepository = postRepository;
        }

        public override async Task<IDataResult<SeoMetadataDto>> AddAsync(SeoMetadataCreateDto dto)
        {
            try
            {
                var entity = _mapper.Map<SeoMetadata>(dto);
                entity.CanonicalGroupId = Guid.NewGuid();
                await _repository.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                var resultDto = _mapper.Map<SeoMetadataDto>(entity);
                return new SuccessDataResult<SeoMetadataDto>(resultDto, "Başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<SeoMetadataDto>("Ekleme sırasında hata oluştu: " + ex.Message);
            }
        }

        public async Task<IDataResult<SeoMetadataDto>> AddVariantAsync(Guid canonicalSeoId, SeoMetadataCreateDto dto)
        {
            try
            {
                var canonical = await _repository.GetByIdAsync(canonicalSeoId);
                if (canonical == null)
                    return new ErrorDataResult<SeoMetadataDto>("Seo kaydı bulunamadı.");

                var exists = await _repository.AnyAsync(x =>
                    x.CanonicalGroupId == canonical.CanonicalGroupId && x.LanguageCode == dto.LanguageCode);
                if (exists)
                    return new ErrorDataResult<SeoMetadataDto>("Bu dil için varyant zaten mevcut.");

                var entity = _mapper.Map<SeoMetadata>(dto);
                entity.CanonicalGroupId = canonical.CanonicalGroupId;

                await _repository.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var resultDto = _mapper.Map<SeoMetadataDto>(entity);
                return new SuccessDataResult<SeoMetadataDto>(resultDto);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<SeoMetadataDto>("Varyant eklenirken hata oluştu: " + ex.Message);
            }
        }

        public async Task<List<SeoMetadataDto>> GetVariantsByPostIdAsync(Guid postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null || post.SeoMetadataId == null)
                return new List<SeoMetadataDto>();

            var canonicalSeo = await _repository.GetByIdAsync(post.SeoMetadataId.Value);
            if (canonicalSeo == null)
                return new List<SeoMetadataDto>();

            var variants = await _repository.GetAllAsync(
                x => x.CanonicalGroupId == canonicalSeo.CanonicalGroupId
            );

            return _mapper.Map<List<SeoMetadataDto>>(variants);
        }
    }
}
