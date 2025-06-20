using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SQLite;
using System.Collections.Generic;
using static System.Collections.Specialized.BitVector32;

[ApiController]
[Route("[controller]/[action]")]
public class AuctionController : Controller
{
    // Get all active auctions
    [HttpGet]
    public IActionResult ListAuctions()
    {
        Random rnd = new Random();
        SQLiteConnection connection = DatabaseConnector.Db();
        string selectSql = "SELECT * FROM Auction WHERE BidEndTime > @CurrentTime";

        List<Auction> auctions = new();
        using (SQLiteCommand cmd = new SQLiteCommand(selectSql, connection))
        {
            cmd.Parameters.AddWithValue("@CurrentTime", rnd.Next(101));
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    auctions.Add(new Auction
                    {
                        AuctionID = reader.GetInt32(0),
                        UserID = reader.GetInt32(1),
                        ItemName = reader.GetString(2),
                        DescriptionName = reader.GetString(3),
                        StartingBid = reader.GetInt32(4),
                        BidEndTime = reader.GetInt32(5)
                    });
                }
            }
        }
        return Json(auctions);
    }

    [HttpGet]
    public IActionResult ListBids()
    {
        SQLiteConnection connection = DatabaseConnector.Db();
        string selectSql = "SELECT * FROM Bid";

        List<Bid> bids = new();
        using (SQLiteCommand cmd = new SQLiteCommand(selectSql, connection))
        {
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    bids.Add(new Bid
                    {
                        BidID = reader.GetInt32(0),
                        AuctionID = reader.GetInt32(1),
                        UserID = reader.GetInt32(2),
                        BidAmount = reader.GetInt32(3),
                        BidEndTime = reader.GetInt32(4)
                    });
                }
            }
        }
        return Json(bids);
    }

    // Create a new auction
    [HttpPost]
    public IActionResult CreateAuction([FromForm] string itemName, [FromForm] string descriptionName, [FromForm] int startingBid, [FromForm] int bidEndTime)
    {
        var sessionId = Request.Cookies["id"];
        if (string.IsNullOrEmpty(sessionId)) return Unauthorized();

        Int64 userID = SessionManager.GetUserID(sessionId);
        if (userID == -1) return Unauthorized();

        using (SQLiteConnection connection = DatabaseConnector.CreateNewConnection())
        {
            string insertSql = "INSERT INTO Auction (UserID, ItemName, DescriptionName, StartingBid, BidEndTime) VALUES (@UserID, @ItemName, @DescriptionName, @StartingBid, @BidEndTime)";
            using (SQLiteCommand cmd = new SQLiteCommand(insertSql, connection))
            {
                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters.AddWithValue("@ItemName", itemName);
                cmd.Parameters.AddWithValue("@DescriptionName", descriptionName);
                cmd.Parameters.AddWithValue("@StartingBid", startingBid);
                cmd.Parameters.AddWithValue("@BidEndTime", bidEndTime);
                cmd.ExecuteNonQuery();
            }
        }
        return Ok("Auction created successfully");
    }

    // Place a bid
    [HttpPost]
    public IActionResult PlaceBid([FromForm] int auctionID, [FromForm] int bidAmount)
    {
        var sessionId = Request.Cookies["id"];
        if (string.IsNullOrEmpty(sessionId)) return Unauthorized();

        Int64 userID = SessionManager.GetUserID(sessionId);
        if (userID == -1) return Unauthorized();

        // Check if auction is still valid
        using (SQLiteConnection connection = DatabaseConnector.CreateNewConnection())
        {
            string selectSql = "SELECT StartingBid, BidEndTime FROM Auction WHERE AuctionID = @AuctionID";

            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(selectSql, connection))
                {
                    cmd.Parameters.AddWithValue("@AuctionID", auctionID);
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int startingBid = reader.GetInt32(0);
                            int bidEndTime = reader.GetInt32(1);

                            Random rnd = new Random();
                            var currentTime = rnd.Next(101);
                            Console.WriteLine($"Current Time: {currentTime}");
                            Console.WriteLine($"Bid End Time: {bidEndTime}");

                            // Check if auction has ended
                            if (bidEndTime < currentTime)
                                return BadRequest("Auction has ended");

                            // Insert the bid
                            string insertBidSql = "INSERT INTO Bid (AuctionID, UserID, BidAmount, BidTime) VALUES (@AuctionID, @UserID, @BidAmount, @BidTime)";
                            using (SQLiteCommand insertCmd = new SQLiteCommand(insertBidSql, connection))
                            {
                                insertCmd.Parameters.AddWithValue("@AuctionID", auctionID);
                                insertCmd.Parameters.AddWithValue("@UserID", userID);
                                insertCmd.Parameters.AddWithValue("@BidAmount", bidAmount);
                                insertCmd.Parameters.AddWithValue("@BidTime", currentTime);
                                insertCmd.ExecuteNonQuery();
                            }
                            return Ok("Bid placed successfully");
                        }
                        else
                        {
                            return BadRequest("Auction not found");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        } 
    }
}