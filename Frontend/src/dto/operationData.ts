export default class OperationData {
	id?: string | null;

	date?: Date;
	kind?: string | null;
	currency?: string | null;

	/** @format double */
	amount?: number;
	category?: string | null;

	brokerName = '';
	accountName = '';
	assetIsin = '';
}
