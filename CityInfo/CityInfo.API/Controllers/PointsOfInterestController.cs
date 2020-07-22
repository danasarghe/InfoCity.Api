using AutoMapper;
using CityInfo.API.Model;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities/{cityid}/pointsofinterest")]
    public class PointsOfInterestController:ControllerBase
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
            IMailService mailService, ICityInfoRepository cityInfoRepository,
            IMapper mapper)
        {
            _logger = logger ?? 
                throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? 
                throw new ArgumentNullException(nameof(mailService));
            _cityInfoRepository = cityInfoRepository ?? 
                throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(cityInfoRepository));
        }

        [HttpGet]
            public IActionResult GetPointsOfInterest(int cityId)
            {
            try
            {
                if (!_cityInfoRepository.CityExists(cityId))
                {
                    _logger.LogInformation($"City with id {cityId} wasn't found when" +
                        $" accesing points of interest.");
                    return NotFound();
                }

                // throw new Exception("Exception example"); 

                var pointsOfInterestForCity = _cityInfoRepository.GetPointsOfInterestFotCity(cityId);

                return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity));
              
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting points of interest for city with id {cityId}. ", ex);
                return StatusCode(500, "A problem happened while handling your request");
            }
            }

            [HttpGet("{id}", Name = "GetPointOfInterest")]
            public IActionResult GetPointOfInterest(int cityId, int id)
            {
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }
            var pointOfInterest = _cityInfoRepository.GetPointOfInterestFotCity(cityId, id);

            if (pointOfInterest == null)
            {
                return NotFound();
            }
           
            return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterest));
            }

            [HttpPost]
            public IActionResult CreatePointOfInterest(int cityId,
                [FromBody] PointOfInterestForCreationDto pointOfInterest)
            {
                if (pointOfInterest.Description == pointOfInterest.Name)
                {
                    ModelState.AddModelError(
                        "Description",
                        "The provided description should be different from the name.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var finalPointOfInterest = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);

            _cityInfoRepository.AddPointOfInterestForCity(cityId, finalPointOfInterest);
            _cityInfoRepository.Save();

            var createdPointOfInterestToReturn = _mapper.Map<Model.PointOfInterestDto>(finalPointOfInterest);

                return CreatedAtRoute(
                    "GetPointOfInterest",
                    new { cityId, id = createdPointOfInterestToReturn.Id },
                    createdPointOfInterestToReturn);
            }

            [HttpPut("{id}")]
            public IActionResult UpdatePointOfInterest(int cityId, int id,
                [FromBody] PointOfInterestForUpdateDto pointOfInterest)
            {
                if (pointOfInterest.Description == pointOfInterest.Name)
                {
                    ModelState.AddModelError(
                        "Description",
                        "The provided description should be different from the name.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestFotCity(cityId, id);
                if (pointOfInterestEntity == null)
                {
                    return NotFound();
                }

            _mapper.Map(pointOfInterest, pointOfInterestEntity);

            _cityInfoRepository.UpdatePointOfInterestForCity(cityId, pointOfInterestEntity);

            _cityInfoRepository.Save();

                return NoContent();
            }

            [HttpPatch("{id}")]
            public IActionResult PartiallyUpdatePointOfInterest(int cityId, int id,
                [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc)
            {
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = _cityInfoRepository
                .GetPointOfInterestFotCity(cityId, id);

            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }
            var pointOfInterestToPatch =
                   _mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

                patchDoc.ApplyTo(pointOfInterestToPatch, ModelState);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (pointOfInterestToPatch.Description == pointOfInterestToPatch.Name)
                {
                    ModelState.AddModelError(
                        "Description",
                        "The provided description should be different from the name.");
                }

                if (!TryValidateModel(pointOfInterestToPatch))
                {
                    return BadRequest(ModelState);
                }

            _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

            _cityInfoRepository.UpdatePointOfInterestForCity(cityId, pointOfInterestEntity);

            _cityInfoRepository.Save();

                
                return NoContent();
            }

            [HttpDelete("{id}")]
            public IActionResult DeletePointOfInterest(int cityId, int id)
            {
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity =
                _cityInfoRepository.GetPointOfInterestFotCity(cityId, id);
            if(pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _cityInfoRepository.DeletePointOfInterestForCity(pointOfInterestEntity);

            _cityInfoRepository.Save();

            _mailService.Send("Point of interest deleted.",
                $"Point of interest {pointOfInterestEntity.Name} " +
                $"with id {pointOfInterestEntity.Id} was deleted");

                return NoContent();
            }
        }
}
