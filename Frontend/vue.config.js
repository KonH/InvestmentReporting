module.exports = {
	devServer: {
		proxy: 'http://localhost:8082',
	},
	chainWebpack: (config) => {
		config.plugin('html').tap((args) => {
			args[0].title = 'Investment Reporting';
			return args;
		});
	},
};
