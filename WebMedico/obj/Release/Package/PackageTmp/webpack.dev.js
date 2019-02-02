var path = require("path");

var webpack = require("webpack");

var CopyWebpackPlugin = require("copy-webpack-plugin");
var CleanWebpackPlugin = require("clean-webpack-plugin");

console.log("@@@@@@@@@ USING DEVELOPMENT @@@@@@@@@@@@@@@");

module.exports = {

    devtool: "source-map",
    performance: {
        hints: false
    },
    entry: {
        'polyfills': "./Angular/polyfills.ts",
        'app': "./Angular/main.ts"
    },

    output: {
        path: __dirname + "/Scripts/",
        filename: "dist/[name].bundle.js",
        chunkFilename: "dist/[id].chunk.js",
        publicPath: ""
    },

    resolve: {
        extensions: [".ts", ".js", ".json", ".css", ".html"]
    },

    devServer: {
        historyApiFallback: true,
        contentBase: path.join(__dirname, "/Scripts/"),
        watchOptions: {
            aggregateTimeout: 300,
            poll: 1000
        }
    },

    module: {
        rules: [
            {
                test: /\.ts$/,
                loaders: [
                    "awesome-typescript-loader",
                    "angular-router-loader",
                    "angular2-template-loader",
                    "source-map-loader",
                    "tslint-loader"
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
                loaders: ["style-loader", "css-loader"]
            },
            {
                test: /\.html$/,
                loader: "raw-loader"
            }
        ],
        exprContextCritical: false
    },
    plugins: [
        new webpack.ProvidePlugin({ $: "jquery", jQuery: "jquery" }),
        new webpack.optimize.CommonsChunkPlugin({ name: ["vendor", "polyfills"] }),

        new CleanWebpackPlugin(
            [
                "./Scripts/dist",
                "./Scripts/assets"
            ]
        ),

        new CopyWebpackPlugin([
            { from: "./Angular/images/*.*", to: "assets/", flatten: true }
        ])
    ]
};

