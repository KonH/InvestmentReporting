export default class Backend {
	static async post(relativeUrl: string, init?: RequestInit) {
		init = init ?? {};
		init.method = 'POST';
		return await this.fetch(relativeUrl, init);
	}

	static async fetch(relativeUrl: string, init?: RequestInit) {
		const result = await fetch(relativeUrl, init);
		if (process.env.NODE_ENV == 'development') {
			console.log(result);
		}
		return result;
	}
}
