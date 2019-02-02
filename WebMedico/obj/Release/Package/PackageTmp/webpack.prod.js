var path = require("path");

var webpack = require("webpack");

var CopyWebpackPlugin = require("copy-webpack-plugin");
var CleanWebpackPlugin = require("clean-webpack-plugin");

console.log("@@@@@@@@@ USING PRODUCTION @@@@@@@@@@@@@@@");

module.exports = {

    entry: {
        'polyfills': "./Angular/polyfills.ts",
        'app': "./Angular/main-aot.ts" // AoT compilation
    },

    output: {
        path: __dirname + "/Scripts/",
        filename: "dist/[name].[hash].bundle.js",
        chunkFilename: "dist/[id].[hash].chunk.js",
        publicPath: ""
    },

    resolve: {
        extensions: [".ts", ".js", ".json", ".css", ".scss", ".html"]
    },

    devServer: {
        historyApiFallback: true,
        stats: "minimal",
        outputPath: path.join(__dirname, "Scripts/")
    },

    module: {
        rules: [
            {
                test: /\.ts$/,
                loaders: [
                    "awesome-typescript-loader",
                    "angular-router-loader?aot=true&genDir=aot/"
                ]
            },
            {
                test: /\.(png|jpg|gif|woff|woff2|ttf|svg|eot)$/,
                loader: "file-loader?name=assets/[name]-[hash:6].[ext]"
            },
            {
                test: /favicon.ico$/,
                loader: "file-loader?name=/[name].[ext]"
            },
            {
                test: /\.css$/,
                loader: "style-loader!css-loader"
            },
            {
                test: /\.scss$/,
                exclude: /node_modules/,
                loaders: ["style-loader", "css-loader", "sass-loader"]
            },
            {
                test: /\.html$/,
                loader: "raw-loader"
            }
        ],
        exprContextCritical: false
    },

    plugins: [
        new CleanWebpackPlugin(
            [
                "./Scripts/dist",
                "./Scripts/assets"
            ]
        ),
        new webpack.NoEmitOnErrorsPlugin(),
        new webpack.optimize.UglifyJsPlugin({
            compress: {
                warnings: false
            },
            output: {
                comments: false
            },
            sourceMap: false
        }),
        new webpack.optimize.CommonsChunkPlugin(
        {
            name: ["vendor", "polyfills"]
        }),

        new CopyWebpackPlugin([
            { from: "./Angular/images/*.*", to: "assets/", flatten: true }
        ])
    ]
};
