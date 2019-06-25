
using Microsoft.ML.Data;

namespace PersonalizedRanking.DataStructures
{
    // Representation of the prediction made by the model (e.g. ranker).
    public class HotelPrediction
    {
        // Maps to the "Srch_Id" column in the underlying Expedia dataset; this is the id of the search\query.
        public uint GroupId { get; set; }

        // The "Label" does not exist in the underlying Expedia dataset and is added in the sample to indicate the ideal rank of a hotel search result.  This is 2 if the user purchased\booked a room at this hotel.  Or, is 1 if the user clicked through to see more information on this hotel.  Otherwise, is 0 if the user neither clicked nor purchased\booked a room at this hotel. 
        public uint Label { get; set; }

        // Prediction made by the model that indicates the relative ranking of the hotel search result.
        [ColumnName("Score")]
        public float PredictedRank { get; set; }

        // Values that are influential in determining the relevance of a data instance. This is a vector that contains concatenated columns from the underlying Expedia dataset.
        public float[] Features { get; set; }
    }
}
