# NetElevation

An APi to get the altitude of any point on the globe

## Doc

This API is quite simple. You can either use a GET request to retrieve the altitude of a single locationOr you can retrieve the altitude of many locations in a single post request.

### GET /elevation?latitude=45.76&longitude=4.82

returns the altitude of the location at latitude and longitude

### POST /elevation

payload: an array of coordinates [ { latitude: 45.76, longitude: 4.82 } ]
returns: an array of coordinates with altitude [ { latitude: 45.76, longitude: 4.82, elevation: 263 } ]

## TODO

- [x] Create a docker image for raspberry pi
- [x] Test docker image on raspberry pi
- [x] Create a controller compatible Open Elevation API
- [x] Create a ZipRepository that load its data from a giant Zip file
- [x] Write some docs
- [ ] Write better docs
- [ ] replace doubles with floats
- [ ] Buy a domain and create a certificate on lets encrypt
- [ ] Create a website
- [ ] Create a command line tool to split Geotiff into smaller files
