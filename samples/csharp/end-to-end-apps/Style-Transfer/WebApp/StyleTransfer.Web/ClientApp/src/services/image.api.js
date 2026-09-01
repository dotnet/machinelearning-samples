import axios from 'axios';

export const getProcessedImage = imageAndFilter => {
  return new Promise((resolve, reject) => {
    axios.post('/api', imageAndFilter)
        .then((response) => resolve(response))
        .catch((error) => reject(error));
  })
};