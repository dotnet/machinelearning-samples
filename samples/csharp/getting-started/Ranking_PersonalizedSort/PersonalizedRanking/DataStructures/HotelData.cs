using Microsoft.ML.Data;
using System;

namespace PersonalizedRanking.DataStructures
{
    // Representation of the Expedia data set: https://www.kaggle.com/c/expedia-personalized-sort/data.  Specifically, this is used for training and testing the model.
    public class HotelData
    {
        // Maps to the "Srch_Id" column; this is the id of the search\query.
        [LoadColumn(0)]
        public uint GroupId { get; set; }

        // Maps to the "Date_Time" column; this is the date\time of the search.
        [LoadColumn(1)]
        public DateTime Srch_DateTime { get; set; }

        // Maps to the "Site_Id" column; this is the id of the Expedia point of sale (e.g. Expedia.com, Expedia.co.uk, etc.)
        [LoadColumn(2)]
        public float Site_Id { get; set; }

        // Maps to the "Visitor_Location_Country_Id" column; this is the id of the country the customer is located.
        [LoadColumn(3)]
        public float Visitor_Location_Country_Id { get; set; }

        // Mpas to the "Visitor_Hist_Starrating" column; this is the mean star rating of hotels the customer has previously purchased; null signifies there is no purchase history on the customer.
        [LoadColumn(4)]
        public float Visitor_Hist_Star_Rating { get; set; }

        // Maps to the "Visitor_Hist_Adr_USD" column; this is the mean price per night (in USD) of the hotesl the customer has previously puchases; null signifies there is no purchase history on the customer.
        [LoadColumn(5)]
        public float Visitor_Hist_Adr_USD { get; set; }

        // Maps to the "Prop_Country_Id" column; this is the id of the country the hotel is located in.
        [LoadColumn(6)]
        public float Prop_Country_Id { get; set; }

        // Maps to the "Prop_Id" column; this is the id of the hotel.
        [LoadColumn(7)]
        public float Prop_Id { get; set; }

        // Maps to the Prop_Starrating" column; this is the star rating of the hotel, from 1 to 5 in increments of 1.  A 0 indicates the property has no starts, the star rating is not known or cannobe be publicized.
        [LoadColumn(8)]
        public float Prop_Star_Rating { get; set; }

        // Maps to the "Prop_Review_Score" column; this is the mean customer review score for the hotel on a scale out of 5, rounded to 0.5 increments.  A 0 means there have been no reviews, null that the information is not available.
        [LoadColumn(9)]
        public float Prop_Review_Score { get; set; }

        // Maps to the "Prop_Bran_Bool" column; this has +1 if the hotel is part of a major hotel chain; 0 if it is an independent hotel.
        [LoadColumn(10)]
        public float Prop_Brand { get; set; }

        // Maps to the "Prop_Location_Score1" column; this is the first score outlining the desirability of a hotel's location.
        [LoadColumn(11)]
        public float Prop_Loc_Score1 { get; set; }

        // Maps to the "Prop_Location_Score2" column; this is the second score outlining the desirability of a hotel's location.
        [LoadColumn(12)]
        public float Prop_Loc_Score2 { get; set; }

        // Maps to the "Prop_Log_Historical_Price" column; this is the logarithm of the mean price of the hotel over the last trading period.  A 0 will occur if the hotel was not sold in that period.
        [LoadColumn(13)]
        public float Prop_Log_Historical_Price { get; set; }

        // Maps to the "Position" column; this is the hotel position in Expedia's search results page.
        [LoadColumn(14)]
        public float Position { get; set; }

        // Maps to the "Price_USD" column; this is the displayed price of the hotel for the given search.  Note that different countries have different conventions regarding displaying taxes and fees and the value may be per night or the whole stay.
        [LoadColumn(15)]
        public float Price_USD { get; set; }

        // Maps to the "Promotion_Flag" column; this has +1 if the hotel had a sale price promotion specifically displayed.
        [LoadColumn(16)]
        public float Promotion_Flag { get; set; }

        // Maps to the "Srch_Destination_Id" column; this is the id of the destination wher the hotel search was performed.
        [LoadColumn(17)]
        public float Srch_Destination_ID { get; set; }

        // Maps to the "Srch_Length_Of_Stay" column; this is the number of nights stay that was searched.
        [LoadColumn(18)]
        public float Srch_Length_Of_Stay { get; set; }

        // Maps to the "Srch_Booking_Window" column; this is the number of days in the future the hotel staty started from the search date.
        [LoadColumn(19)]
        public float Srch_Booking_Window { get; set; }

        // Maps to the "Srch_Adults_Count" column; this is the number of adults specified in the hotel room.
        [LoadColumn(20)]
        public float Srch_Adults_Count { get; set; }

        // Maps to the "Srch_Children_Count" column; this is the number of (extra occupancy) children specified in the hotel room.
        [LoadColumn(21)]
        public float Srch_Children_Count { get; set; }

        // Maps to the "Srch_Room_Count" column; this is the number of hotel rooms specified in the search.
        [LoadColumn(22)]
        public float Srch_Room_Count { get; set; }

        // Maps to the "Srch_Saturday_Night_Bool" column; this has +1 if the stay includs a Saturday night, starts from Thursday within a length of stay is less than or equal to 4 nights (e.g. weekend) - otherwise 0.
        [LoadColumn(23)]
        public float Srch_Saturday_Night { get; set; }

        // Maps to the "Srch_Query_Affility_Score" column; this is the log of the probability a hotel will be clicked on in internet searches (hence the values are negative).  Null signifies there is no data (e.g. hotel did not register in any searches).
        [LoadColumn(24)]
        public float Srch_Query_Affinity_Score { get; set; }

        // Maps to the "Orig_Destination_Distance"; this is the physical distance between the hotel and the customer at the time of the search.  A null means the distance could not be calculated.
        [LoadColumn(25)]
        public float Orig_Destination_Distance { get; set; }

        // Maps to the "Random_Bool" column; this is +1 when the displayed sort was random - 0 when the noraml sort order (determined by Expedia's algorithm) was displayed
        [LoadColumn(26)]
        public float Random_Position { get; set; }

        // Maps to the "Comp1_Rate" column; this is +1 if Expedia has a lwoer price than competitor 1 for the hotel. Or, 0 if the same.  Or, -1 if Expedia's price is higher than competitor 1.  Null signifies there is no competitive data.
        [LoadColumn(27)]
        public float Comp1_Rate { get; set; }

        // Maps to the "Comp1_Inv" column; this is +1 if competitor 1 does not have availability in the hotel.  Or, 0 if both Expedia and competitor 1 have availability.  Null signifies there is no competitive data.
        [LoadColumn(28)]
        public float Comp1_Inv { get; set; }

        // Maps to "Comp1_Rate_Percent_Diff" column; this is the absolute percentage difference (if one exists) between Expedia and competitor 1's price (Expedia's price the denominator).  Null signifies there is no competitive data.
        [LoadColumn(29)]
        public float Comp1_Rate_Percent_Diff { get; set; }

        // Maps to the "Comp2_Rate" column; this is +1 if Expedia has a lwoer price than competitor 2 for the hotel. Or, 0 if the same.  Or, -1 if Expedia's price is higher than competitor 2.  Null signifies there is no competitive data.
        [LoadColumn(30)]
        public float Comp2_Rate { get; set; }

        // Maps to the "Comp2_Inv" column; this is +1 if competitor 2 does not have availability in the hotel.  Or, 0 if both Expedia and competitor 2 have availability.  Null signifies there is no competitive data.
        [LoadColumn(31)]
        public float Comp2_Inv { get; set; }

        // Maps to "Comp2_Rate_Percent_Diff" column; this is the absolute percentage difference (if one exists) between Expedia and competitor 2's price (Expedia's price the denominator).  Null signifies there is no competitive data.
        [LoadColumn(32)]
        public float Comp2_Rate_Percent_Diff { get; set; }

        // Maps to the "Comp3_Rate" column; this is +1 if Expedia has a lwoer price than competitor 3 for the hotel. Or, 0 if the same.  Or, -1 if Expedia's price is higher than competitor 3.  Null signifies there is no competitive data.
        [LoadColumn(33)]
        public float Comp3_Rate { get; set; }

        // Maps to the "Comp3_Inv" column; this is +1 if competitor 3 does not have availability in the hotel.  Or, 0 if both Expedia and competitor 3 have availability.  Null signifies there is no competitive data.
        [LoadColumn(34)]
        public float Comp3_Inv { get; set; }

        // Maps to "Comp3_Rate_Percent_Diff" column; this is the absolute percentage difference (if one exists) between Expedia and competitor 3's price (Expedia's price the denominator).  Null signifies there is no competitive data.
        [LoadColumn(35)]
        public float Comp3_Rate_Percent_Diff { get; set; }

        // Maps to the "Comp4_Rate" column; this is +1 if Expedia has a lwoer price than competitor 4 for the hotel. Or, 0 if the same.  Or, -1 if Expedia's price is higher than competitor 4.  Null signifies there is no competitive data.
        [LoadColumn(36)]
        public float Comp4_Rate { get; set; }

        // Maps to the "Comp4_Inv" column; this is +1 if competitor 4 does not have availability in the hotel.  Or, 0 if both Expedia and competitor 4 have availability.  Null signifies there is no competitive data.
        [LoadColumn(37)]
        public float Comp4_Inv { get; set; }

        // Maps to "Comp4_Rate_Percent_Diff" column; this is the absolute percentage difference (if one exists) between Expedia and competitor 4's price (Expedia's price the denominator).  Null signifies there is no competitive data.
        [LoadColumn(38)]
        public float Comp4_Rate_Percent_Diff { get; set; }

        // Maps to the "Comp5_Rate" column; this is +1 if Expedia has a lwoer price than competitor 5 for the hotel. Or, 0 if the same.  Or, -1 if Expedia's price is higher than competitor 5.  Null signifies there is no competitive data.
        [LoadColumn(39)]
        public float Comp5_Rate { get; set; }

        // Maps to the "Comp5_Inv" column; this is +1 if competitor 5 does not have availability in the hotel.  Or, 0 if both Expedia and competitor 5 have availability.  Null signifies there is no competitive data.
        [LoadColumn(40)]
        public float Comp5_Inv { get; set; }

        // Maps to "Comp5_Rate_Percent_Diff" column; this is the absolute percentage difference (if one exists) between Expedia and competitor 5's price (Expedia's price the denominator).  Null signifies there is no competitive data.
        [LoadColumn(41)]
        public float Comp5_Rate_Percent_Diff { get; set; }

        // Maps to the "Comp6_Rate" column; this is +1 if Expedia has a lwoer price than competitor 6 for the hotel. Or, 0 if the same.  Or, -1 if Expedia's price is higher than competitor 6.  Null signifies there is no competitive data.
        [LoadColumn(42)]
        public float Comp6_Rate { get; set; }

        // Maps to the "Comp6_Inv" column; this is +1 if competitor 6 does not have availability in the hotel.  Or, 0 if both Expedia and competitor 6 have availability.  Null signifies there is no competitive data.
        [LoadColumn(43)]
        public float Comp6_Inv { get; set; }

        // Maps to "Comp6_Rate_Percent_Diff" column; this is the absolute percentage difference (if one exists) between Expedia and competitor 6's price (Expedia's price the denominator).  Null signifies there is no competitive data.
        [LoadColumn(44)]
        public float Comp6_Rate_Percent_Diff { get; set; }

        // Maps to the "Comp7_Rate" column; this is +1 if Expedia has a lwoer price than competitor 7 for the hotel. Or, 0 if the same.  Or, -1 if Expedia's price is higher than competitor 7.  Null signifies there is no competitive data.
        [LoadColumn(45)]
        public float Comp7_Rate { get; set; }

        // Maps to the "Com72_Inv" column; this is +1 if competitor 7 does not have availability in the hotel.  Or, 0 if both Expedia and competitor 7 have availability.  Null signifies there is no competitive data.
        [LoadColumn(46)]
        public float Comp7_Inv { get; set; }

        // Maps to "Comp7_Rate_Percent_Diff" column; this is the absolute percentage difference (if one exists) between Expedia and competitor 7's price (Expedia's price the denominator).  Null signifies there is no competitive data.
        [LoadColumn(47)]
        public float Comp7_Rate_Percent_Diff { get; set; }

        // Maps to the "Comp8_Rate" column; this is +1 if Expedia has a lwoer price than competitor 8 for the hotel. Or, 0 if the same.  Or, -1 if Expedia's price is higher than competitor 8.  Null signifies there is no competitive data.
        [LoadColumn(48)]
        public float Comp8_Rate { get; set; }

        // Maps to the "Comp8_Inv" column; this is +1 if competitor 8 does not have availability in the hotel.  Or, 0 if both Expedia and competitor 8 have availability.  Null signifies there is no competitive data.
        [LoadColumn(49)]
        public float Comp8_Inv { get; set; }

        // Maps to "Comp8_Rate_Percent_Diff" column; this is the absolute percentage difference (if one exists) between Expedia and competitor 8's price (Expedia's price the denominator).  Null signifies there is no competitive data.
        [LoadColumn(50)]
        public float Comp8_Rate_Percent_Diff { get; set; }

        // Maps to the "Click_Bool" column; this is +1 if the user clicked through to see more information on this hotel.
        [LoadColumn(51)]
        public float Srch_Result_Clicked { get; set; }

        // Maps to the "Gross_Booking_USD" column; this it eh total value of the transaction.  This can differ from the price_us due to taxes, fees, conventions on multiple day booking and purchase of a room type otehr than the one shown.
        [LoadColumn(52)]
        public float Gross_Bookings_USD { get; set; }

        // Maps to the "Booking_Bool" column; this is +1 if the user purchases a room at this hotel.
        [LoadColumn(53)]
        public float Srch_Result_Booked { get; set; }

        // The "Label" does not exist in the underlying Expedia dataset and is added in the sample to indicate the ideal rank (e.g. predicted value) of a hotel search result.  
        // This is 2 if the user purchased\booked a room at this hotel.  Or, is 1 if the user clicked through to see more information on this hotel.  Otherwise, is 0 if the user neither clicked nor purchased\booked a room at this hotel. 
        [LoadColumn(54)]
        public uint Label { get; set; }
    }
}
