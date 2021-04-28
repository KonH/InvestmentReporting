interface HasProgress {
	isInProgress: boolean;
}

export default class Progress {
	static getClass(holder: HasProgress, base: string) {
		return base + (holder.isInProgress ? ' disabled' : '');
	}

	static async wrap(holder: HasProgress, promise: () => Promise<unknown>) {
		holder.isInProgress = true;
		try {
			await promise();
		} finally {
			holder.isInProgress = false;
		}
	}
}
