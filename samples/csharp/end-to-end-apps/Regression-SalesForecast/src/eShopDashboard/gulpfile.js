/// <binding BeforeBuild='copy-assets' />
// gulpfile.js

var _ = require('lodash'),
    gulp = require('gulp');

var node = "./node_modules/",
    www  = "./wwwroot/lib/";

gulp.task('copy-assets', function () {
    var assets = {
        js: [
            `${node}jquery/dist/jquery.js`,
            `${node}jquery/dist/jquery.min.js`,
            `${node}jquery/dist/jquery.min.map`,
            `${node}bootstrap/dist/js/bootstrap.js`,
            `${node}bootstrap/dist/js/bootstrap.min.js`,
            `${node}bootstrap/dist/js/bootstrap.js.map`,
            `${node}typeahead.js/dist/typeahead.bundle.js`,
            `${node}typeahead.js/dist/typeahead.bundle.min.js`,
            `${node}plotly.js/dist/plotly-basic.js`,
            `${node}plotly.js/dist/plotly-basic.min.js`
        ],
        css: [
            `${node}bootstrap/dist/css/bootstrap.css`,
            `${node}bootstrap/dist/css/bootstrap.map`,
            `${node}bootstrap/dist/css/bootstrap.min.css`,
        ],
        webfonts: [
            `${node}@fortawesome/fontawesome-free-webfonts/webfonts/*`
        ]
    };
    _(assets).forEach(function (assets, type) {
        gulp.src(assets).pipe(gulp.dest(`${www}${type}`));
    });
});



