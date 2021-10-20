using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIGetListeController : ControllerBase
    {
        // GET: api/<APIGetListeController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<APIGetListeController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<APIGetListeController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<APIGetListeController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<APIGetListeController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
