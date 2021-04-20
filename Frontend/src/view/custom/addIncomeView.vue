<template>
	<h1>Add Income</h1>
	<div class="form-group">
		<label>
			Date:
			<input ref="date" type="datetime-local" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Amount:
			<input ref="amount" type="number" class="form-control" value="0" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Category:
			<input ref="category" type="text" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<asset-selector :value="asset" :broker-id="brokerId" :account-id="accountId" @input="onAssetSelect" />
	</div>
	<button :onclick="onclick" class="btn btn-primary">Add</button>
	<router-link to="/custom" class="btn btn-secondary ml-2">Back</router-link>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import { Action, State } from 'vuex-class';
import { AssetDto, StateDto } from '@/api/state';
import { Ref } from 'vue-property-decorator';
import AssetSelector from '@/component/common/assetSelector.vue';

@Options({
	name: 'AddIncomeView',
	components: {
		AssetSelector,
	},
})
export default class AddIncome extends Vue {
	@State('activeState')
	activeState!: StateDto;

	@Ref('date')
	dateInput!: HTMLInputElement;

	@Ref('amount')
	amountInput!: HTMLInputElement;

	@Ref('category')
	categoryInput!: HTMLInputElement;

	asset = '';

	@Action('fetchState')
	fetchState!: () => void;

	get brokerId() {
		return this.$route.params.broker as string;
	}

	get accountId() {
		return this.$route.params.account as string;
	}

	onAssetSelect(value: string) {
		this.asset = value;
	}

	get assets() {
		const broker = this.activeState.brokers?.find((b) => b.id == this.brokerId);
		const viewArray: [AssetDto] = [{ id: '' }];
		const inventory = broker?.inventory ?? [];
		return viewArray.concat(inventory);
	}

	mounted() {
		this.setCurrentDate();
	}

	setCurrentDate() {
		this.dateInput.value = new Date().toISOString();
	}

	async onclick() {
		const result = await Backend.tryFetch(
			Backend.state().income.incomeCreate({
				date: new Date(this.dateInput.value).toISOString(),
				broker: this.brokerId,
				account: this.accountId,
				amount: Number.parseFloat(this.amountInput.value),
				category: this.categoryInput.value,
				asset: this.asset,
			})
		);
		if (result?.ok) {
			this.fetchState();
			await router.push('/custom');
		} else {
			alert(`Failed: ${result?.error}`);
		}
	}
}
</script>
