export default class InputUtils {
	static setCurrentDate(input: HTMLInputElement) {
		// 2021-04-21T00:05:41.590Z => 2021-04-21T00:05:41
		const dateStr = new Date().toISOString();
		input.value = dateStr.slice(0, dateStr.length - 5);
	}
}
