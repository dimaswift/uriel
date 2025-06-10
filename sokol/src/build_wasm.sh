
rm -r -f www
mkdir www
cd www || exit
emcmake cmake -DCMAKE_BUILD_TYPE=MinSizeRel ..
cmake --build .
emrun main.html