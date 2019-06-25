
namespace PersonalizedRanking.DataStructures
{
    // Used by ML .NET to do a custom mapping to add the "Label" column to the dataset.
    public class HotelRelevance
    {
        // The "Label" does not exist in the underlying Expedia dataset and is added in the sample to indicate the ideal rank of a hotel search result.  This is 2 if the user purchased\booked a room at this hotel.  Or, is 1 if the user clicked through to see more information on this hotel.  Otherwise, is 0 if the user neither clicked nor purchased\booked a room at this hotel. 
        public uint Label { get; set; }
    }
}
