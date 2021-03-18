export default class Backend {
	// TODO: replace url in prod
	private static url = 'http://localhost:8082/';

	static async post(relativeUrl: string, init?: RequestInit) {
		init = init ?? {};
		init.method = 'POST';
		return await this.fetch(relativeUrl, init);
	}

	static async fetch(relativeUrl: string, init?: RequestInit) {
		init = init ?? {};
		// TODO: do not use no-cors in prod
		init.mode = 'no-cors';
		const result = await fetch(this.url + relativeUrl, init);
		if (!result.ok) {
			console.log(result);
		}
		return result;
	}
}
