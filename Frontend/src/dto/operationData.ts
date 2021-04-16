export default class OperationData {
	id?: string | null;

	/** @format date-time */
	date?: string;
	kind?: string | null;
	currency?: string | null;

	/** @format double */
	amount?: number;
	category?: string | null;

	brokerName = '';
	accountName = '';
	assetIsin = '';
}
