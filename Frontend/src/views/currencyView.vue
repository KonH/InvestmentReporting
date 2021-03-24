<template>
	<div>
		<h2>Currencies</h2>
		<div v-for="currency in currencies" :key="currency.id">
			<currency :currency="currency" />
		</div>
		<router-link to="/addCurrency" class="btn btn-primary"
			>Add Currency</router-link
		>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Currency from '@/component/currency.vue';
import { StateDto } from '@/api/state';
import { State } from 'vuex-class';

@Options({
	components: {
		Currency,
	},
})
export default class CurrencyView extends Vue {
	@State('activeState') activeState!: StateDto;

	get currencies() {
		return this.activeState.currencies;
	}
}
</script>
