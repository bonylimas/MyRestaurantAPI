using Microsoft.AspNetCore.Mvc;

namespace MyRestaurantAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationController : ControllerBase
    {
        private ApplicationDBContext _db;

        public ReservationController(ApplicationDBContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get All Reservation sorted by Reservation Date/Time
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Reservation> GetAll()
        {

            return _db.Reservations.OrderBy(r=>r.ReservationTime).OrderBy(r=> r.ReservationTime).ToList();

        }

        /// <summary>
        /// Get Reservation by Reservation Id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public Reservation Get(int id)
        {

            return _db.Reservations.FirstOrDefault(r => r.Id == id);

        }

        /// <summary>
        /// Get Reservation by phone number
        /// Example: Reservation/byphone/0415212151
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet("byphone/{phone}")]
        public IEnumerable<Reservation> GetByPhone(string phone)
        {
            IEnumerable<Reservation> reservations = _db.Reservations.Where(r => r.GuestPhone.Contains(phone))
                                                                    .OrderBy(r => r.ReservationTime).ToList();

            return reservations;

        }

        /// <summary>
        /// Get Reservations on the date specified
        /// 
        /// Example: Reservation/bydate/22-02-2022
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet("bydate/{date}")]
        public IEnumerable<Reservation> GetByDate(string date)
        {
            DateTime dateObj = DateTime.Parse(date);
            DateTime fromDate = new DateTime(dateObj.Year, dateObj.Month, dateObj.Day, 0, 0, 0);
            DateTime toDate = new DateTime(dateObj.Year, dateObj.Month, dateObj.Day, 23, 59, 59);

            IEnumerable<Reservation> reservations = _db.Reservations.Where(r => r.ReservationTime >= fromDate && r.ReservationTime <= toDate)
                                                                    .OrderBy(r => r.ReservationTime).ToList();

            return reservations;

        }

        /// <summary>
        /// Create Reservation given the Reservation object
        /// Table Number will be assigned by the API
        /// 
        /// {
        ///     "guestName": "Bony Limas",
        ///     "guestPhone": "0415212151",
        ///     "reservationTime": "2022-02-21T20:30:21.015Z"
        /// }
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns>Id and TableNumber</returns>
        [HttpPost]
        public IActionResult Create(Reservation reservation)
        {
            // Get all the tables fully booked
            IQueryable<Reservation> bookedTable = _db.Reservations.Where(r =>
                (reservation.ReservationTime >= r.ReservationTime && reservation.ReservationTime < r.ReservationTime.AddMinutes(r.DurationMinute)) ||
                (reservation.ReservationTime.AddMinutes(reservation.DurationMinute) > r.ReservationTime && reservation.ReservationTime.AddMinutes(reservation.DurationMinute) < r.ReservationTime.AddMinutes(r.DurationMinute)));

            // Check if it's fullt booked
            if (bookedTable.Count() >= 10)
            {
                ModelState.AddModelError("ReservationTime", "Table for this date and time is fully booked.");
            }

            if (ModelState.IsValid)
            {
                // Create New

                // We need to assign table number from 1 to 10
                int tableNumber = 1;

                for (int i = 1; i <= 10 && bookedTable != null && bookedTable.Count() > 0; i++)
                {
                    tableNumber = i;
                    if (bookedTable.FirstOrDefault(t => t.TableNumber == tableNumber) == null)
                        break;
                }

                reservation.TableNumber = tableNumber;
                reservation.DurationMinute = 60;

                // Add to Database
                _db.Reservations.Add(reservation);
                _db.SaveChanges();

                return CreatedAtAction("Create", new { Id = reservation.Id, TableNumber = reservation.TableNumber, Version = "1.0" });
            }

            return BadRequest("Reservation is not valid");
        }

        /// <summary>
        /// Update Reservation given the Id and Reservation object
        /// 
        /// Example: Reservation/2
        /// 
        /// {
        ///     "id": 2,
        ///     "tableNumber": 2,
        ///     "guestName": "Bony Limas",
        ///     "guestPhone": "0415212151",
        ///     "reservationTime": "2022-02-21T20:30:21.015Z"
        /// }
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reservation"></param>
        /// <returns>Success if updated</returns>
        [HttpPut("{id}")]
        public IActionResult Update(int id, Reservation reservation)
        {
            // Check if Reservation is in db
            Reservation toUpdate = _db.Reservations.FirstOrDefault(r => r.Id == id);

            if (toUpdate == null)
                return BadRequest("Reservation id not found");

            if (id != toUpdate.Id)
                return BadRequest("Reservation id does not match the route's id");

            // Get all the tables fully booked
            IQueryable<Reservation> bookedTable = _db.Reservations.Where(r =>
                (reservation.ReservationTime >= r.ReservationTime && reservation.ReservationTime < r.ReservationTime.AddMinutes(r.DurationMinute)) ||
                (reservation.ReservationTime.AddMinutes(reservation.DurationMinute) > r.ReservationTime && reservation.ReservationTime.AddMinutes(reservation.DurationMinute) < r.ReservationTime.AddMinutes(r.DurationMinute)));

            // Check if it's fullt booked
            if (bookedTable.Count() >= 10)
            {
                ModelState.AddModelError("ReservationTime", "Table for this date and time is fully booked.");
            }

            if (ModelState.IsValid)
            {

                // Update
                
                // Check for table number changes 
                if (bookedTable != null && bookedTable.FirstOrDefault(t => t.TableNumber == toUpdate.TableNumber && t.Id != toUpdate.Id) != null)
                {
                    int tableNumber = 1;

                    for (int i = 1; i <= 10 && bookedTable != null && bookedTable.Count() > 0; i++)
                    {
                        tableNumber = i;
                        if (bookedTable.FirstOrDefault(t => t.TableNumber == tableNumber) == null)
                            break;
                    }


                    toUpdate.TableNumber = tableNumber;
                }

                toUpdate.GuestName = reservation.GuestName;
                toUpdate.GuestPhone = reservation.GuestPhone;
                toUpdate.ReservationTime = reservation.ReservationTime;
                toUpdate.DurationMinute = 60;

                // Update the selected
                _db.Reservations.Update(toUpdate);
                _db.SaveChanges();


                return NoContent();
            }

            return BadRequest("Reservation is not valid");
        }

        /// <summary>
        /// Delete / Cancel Reservation given the Id
        /// 
        /// Example: Reservation/2
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Success if deleted</returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            // Check if Reservation is in db
            Reservation toDelete = _db.Reservations.FirstOrDefault(r => r.Id == id);

            if (toDelete == null)
                return NotFound("Reservation id not found");

            _db.Reservations.Remove(toDelete);
            _db.SaveChanges();

            return NoContent();
        }
    }
}