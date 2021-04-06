<template>
	<router-link to="/" class="btn btn-primary">Back</router-link>
	<table class="table">
		<thead>
			<tr>
				<th>Date</th>
				<th>Amount</th>
				<th>Kind</th>
				<th>Category</th>
				<th>Asset</th>
			</tr>
		</thead>
		<tr v-for="operation in operations" :key="operation.date">
			<operation :operation="operation" />
		</tr>
	</table>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { OperationDto, StateDto } from '@/api/state';
import Operation from '@/component/operation.vue';
import Backend from '@/service/backend';
import { State } from 'vuex-class';

@Options({
	name: 'OperationList',
	components: {
		Operation,
	},
})
export default class OperationList extends Vue {
	@State('activeState')
	activeState!: StateDto;

	operations: OperationDto[] = [];

	get brokerId() {
		return this.$route.params.broker as string;
	}

	async created() {
		const accountId = this.$route.params.account as string;
		const startDate = new Date(1, 1, 1).toISOString();
		const endDate = new Date().toISOString();
		const response = await Backend.state().operation.forAccountList({
			startDate: startDate,
			endDate: endDate,
			broker: this.brokerId,
			account: accountId,
		});
		const operations = response.data;
		for (const operation of operations) {
			operation.asset = this.getAssetName(operation.asset);
		}
		this.operations = operations;
	}

	getAssetName(assetId: string | null | undefined) {
		if (!assetId) {
			return '';
		}
		const broker = this.activeState.brokers?.find((b) => b.id == this.brokerId);
		const asset = broker?.inventory?.find((a) => a.id == assetId);
		if (asset) {
			return `${asset.name} (${asset.isin})`;
		}
		return '';
	}
}
</script>
