using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteTests
{
  public class TestEntity
  {
    [Key]
    public long Id { get; set; }

    public string Information { get; set; }
  }
}
