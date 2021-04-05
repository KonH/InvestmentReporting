<template>
	<router-link to="/" class="btn btn-primary">Back</router-link>
	<table class="table">
		<thead>
			<tr>
				<th>Date</th>
				<th>Amount</th>
				<th>Kind</th>
				<th>Category</th>
			</tr>
		</thead>
		<tr v-for="operation in operations" :key="operation.date">
			<operation :operation="operation" />
		</tr>
	</table>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { OperationDto } from '@/api/state';
import Operation from '@/component/operation.vue';
import Backend from '@/service/backend';

@Options({
	name: 'AssetOperationList',
	components: {
		Operation,
	},
})
export default class AssetOperationList extends Vue {
	brokerId!: string;
	accountId!: string;
	operations: OperationDto[] = [];

	async created() {
		const brokerId = this.$route.params.broker as string;
		const assetId = this.$route.params.asset as string;
		const startDate = new Date(1, 1, 1).toISOString();
		const endDate = new Date().toISOString();
		const response = await Backend.state().operation.forAssetList({
			startDate: startDate,
			endDate: endDate,
			broker: brokerId,
			asset: assetId,
		});
		this.operations = response.data;
	}
}
</script>
